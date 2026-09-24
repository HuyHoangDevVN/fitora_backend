using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.Services.IServices;

namespace InteractService.Application.Usecases.ShortClips;

internal static class OptionalAuth
{
    // List/GetById không có [Authorize] (xem cho khách) — GetUserFromClaimToken() throw
    // khi không có token, nên không thể dùng trực tiếp ở đây.
    public static Guid? TryGetUserId(IAuthorizeExtension auth)
    {
        try { return auth.GetUserFromClaimToken().Id; }
        catch { return null; }
    }
}

public record CreateShortClipCommand(string VideoUrl, string? ThumbnailUrl, string Caption, int Duration) : ICommand<ShortClipDto>;
public record GetShortClipByIdQuery(Guid Id) : IQuery<ShortClipDto>;
public record GetShortClipsQuery(int PageIndex = 0, int PageSize = 20) : IQuery<PaginatedDto<ShortClipDto>>;
public record DeleteShortClipCommand(Guid Id) : ICommand<ResponseDto>;
public record ShortClipDto(Guid Id, Guid AuthorId, string VideoUrl, string? ThumbnailUrl, string Caption, int Duration, string Visibility, string Status, DateTime? CreatedAt, string? AuthorUsername = null, string? AuthorAvatarUrl = null);
public record PaginatedDto<T>(List<T> Items, int TotalCount, int PageIndex, int PageSize);

public class ShortClipHandlers(IApplicationDbContext db, IAuthorizeExtension auth, IUserInfoBatchService userInfoBatchService)
    : ICommandHandler<CreateShortClipCommand, ShortClipDto>,
      IQueryHandler<GetShortClipByIdQuery, ShortClipDto>,
      IQueryHandler<GetShortClipsQuery, PaginatedDto<ShortClipDto>>,
      ICommandHandler<DeleteShortClipCommand, ResponseDto>
{
    public async Task<ShortClipDto> Handle(CreateShortClipCommand c, CancellationToken ct)
    {
        if (c.Duration > 180) throw new BuildingBlocks.Exceptions.BadRequestException("Duration max 180s");
        if (c.Caption?.Length > 500) throw new BuildingBlocks.Exceptions.BadRequestException("Caption max 500 chars");
        var userId = auth.GetUserFromClaimToken().Id;
        var e = new ShortClip
        {
            Id = Guid.NewGuid(), AuthorId = userId, VideoUrl = c.VideoUrl, ThumbnailUrl = c.ThumbnailUrl,
            Caption = c.Caption ?? "", Duration = c.Duration, Visibility = "Public", Status = "Published", CreatedAt = DateTime.UtcNow
        };
        db.ShortClips.Add(e);
        await db.SaveChangesAsync(ct);
        var authorInfo = await userInfoBatchService.GetUserDisplayInfosAsync(userId, new List<Guid> { userId }, ct);
        return Map(e, authorInfo);
    }
    public async Task<ShortClipDto> Handle(GetShortClipByIdQuery q, CancellationToken ct)
    {
        var e = await db.ShortClips.FirstOrDefaultAsync(x => x.Id == q.Id, ct) ?? throw new BuildingBlocks.Exceptions.NotFoundException("ShortClip not found");
        var requestUserId = OptionalAuth.TryGetUserId(auth);
        var authorInfo = await userInfoBatchService.GetUserDisplayInfosAsync(requestUserId, new List<Guid> { e.AuthorId }, ct);
        return Map(e, authorInfo);
    }
    public async Task<PaginatedDto<ShortClipDto>> Handle(GetShortClipsQuery q, CancellationToken ct)
    {
        var published = db.ShortClips.Where(x => x.Status == "Published" && x.Visibility == "Public");
        var total = await published.CountAsync(ct);
        var items = await published.OrderByDescending(x => x.CreatedAt).Skip(q.PageIndex * q.PageSize).Take(q.PageSize).ToListAsync(ct);
        var requestUserId = OptionalAuth.TryGetUserId(auth);
        var authorIds = items.Select(x => x.AuthorId).Distinct().ToList();
        var authorInfo = await userInfoBatchService.GetUserDisplayInfosAsync(requestUserId, authorIds, ct);
        return new(items.Select(x => Map(x, authorInfo)).ToList(), total, q.PageIndex, q.PageSize);
    }
    public async Task<ResponseDto> Handle(DeleteShortClipCommand c, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var e = await db.ShortClips.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new BuildingBlocks.Exceptions.NotFoundException("ShortClip not found");
        if (e.AuthorId != userId) throw new BuildingBlocks.Exceptions.UnAuthorizationException("Not author");
        db.ShortClips.Remove(e);
        await db.SaveChangesAsync(ct);
        return new ResponseDto(Message: "Deleted");
    }
    private static ShortClipDto Map(ShortClip e, Dictionary<Guid, UserDisplayInfo> authorInfo)
    {
        authorInfo.TryGetValue(e.AuthorId, out var author);
        return new(e.Id, e.AuthorId, e.VideoUrl, e.ThumbnailUrl, e.Caption, e.Duration, e.Visibility, e.Status, e.CreatedAt, author?.Username, author?.ProfilePictureUrl);
    }
}

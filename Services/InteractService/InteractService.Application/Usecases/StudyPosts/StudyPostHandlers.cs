using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;

namespace InteractService.Application.Usecases.StudyPosts;

public record CreateStudyPostCommand(string Title, string Content, Guid? CategoryId, List<string>? AttachmentUrls) : ICommand<StudyPostDto>;
public record UpdateStudyPostCommand(Guid Id, string Title, string Content) : ICommand<StudyPostDto>;
public record DeleteStudyPostCommand(Guid Id) : ICommand<ResponseDto>;
public record GetStudyPostByIdQuery(Guid Id) : IQuery<StudyPostDto>;
public record GetStudyPostsQuery(int PageIndex = 0, int PageSize = 20) : IQuery<PaginatedDto<StudyPostDto>>;
public record StudyPostDto(Guid Id, Guid AuthorId, string Title, string Content, Guid? CategoryId, List<string> AttachmentUrls, string Status, DateTime? CreatedAt, DateTime? UpdatedAt);
public record PaginatedDto<T>(List<T> Items, int TotalCount, int PageIndex, int PageSize);

public class StudyPostHandlers(
    IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<CreateStudyPostCommand, StudyPostDto>,
      ICommandHandler<UpdateStudyPostCommand, StudyPostDto>,
      ICommandHandler<DeleteStudyPostCommand, ResponseDto>,
      IQueryHandler<GetStudyPostByIdQuery, StudyPostDto>,
      IQueryHandler<GetStudyPostsQuery, PaginatedDto<StudyPostDto>>
{
    public async Task<StudyPostDto> Handle(CreateStudyPostCommand c, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var e = new StudyPost
        {
            Id = Guid.NewGuid(), AuthorId = userId, Title = c.Title, Content = c.Content,
            CategoryId = c.CategoryId, AttachmentUrlsJson = System.Text.Json.JsonSerializer.Serialize(c.AttachmentUrls ?? new()),
            Status = "Published", CreatedAt = DateTime.UtcNow, LastModified = DateTime.UtcNow
        };
        db.StudyPosts.Add(e);
        await db.SaveChangesAsync(ct);
        return Map(e);
    }
    public async Task<StudyPostDto> Handle(UpdateStudyPostCommand c, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var e = await db.StudyPosts.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new BuildingBlocks.Exceptions.NotFoundException("StudyPost not found");
        if (e.AuthorId != userId) throw new BuildingBlocks.Exceptions.UnAuthorizationException("Not author");
        e.Title = c.Title; e.Content = c.Content; e.LastModified = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(e);
    }
    public async Task<ResponseDto> Handle(DeleteStudyPostCommand c, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var e = await db.StudyPosts.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new BuildingBlocks.Exceptions.NotFoundException("StudyPost not found");
        if (e.AuthorId != userId) throw new BuildingBlocks.Exceptions.UnAuthorizationException("Not author");
        db.StudyPosts.Remove(e);
        await db.SaveChangesAsync(ct);
        return new ResponseDto(Message: "Deleted");
    }
    public async Task<StudyPostDto> Handle(GetStudyPostByIdQuery q, CancellationToken ct)
    {
        var e = await db.StudyPosts.FirstOrDefaultAsync(x => x.Id == q.Id, ct) ?? throw new BuildingBlocks.Exceptions.NotFoundException("StudyPost not found");
        return Map(e);
    }
    public async Task<PaginatedDto<StudyPostDto>> Handle(GetStudyPostsQuery q, CancellationToken ct)
    {
        var total = await db.StudyPosts.CountAsync(ct);
        var items = await db.StudyPosts.OrderByDescending(x => x.CreatedAt).Skip(q.PageIndex * q.PageSize).Take(q.PageSize).ToListAsync(ct);
        return new(items.Select(Map).ToList(), total, q.PageIndex, q.PageSize);
    }
    private static StudyPostDto Map(StudyPost e)
    {
        List<string> urls;
        try { urls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(e.AttachmentUrlsJson) ?? new(); } catch { urls = new(); }
        return new(e.Id, e.AuthorId, e.Title, e.Content, e.CategoryId, urls, e.Status, e.CreatedAt, e.LastModified);
    }
}

using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.Shares;

/// <summary>
/// Chia sẻ = tạo 1 Post mới của người chia sẻ (nội dung có thể kèm caption riêng),
/// trỏ về bài gốc qua bảng Shares. Tái dùng toàn bộ pipeline Post hiện có (feed/vote/comment).
/// </summary>
public record CreateShareCommand(Guid OriginalPostId, string? Caption, int ShareTo) : ICommand<ShareDto>;
public record GetSharesCountQuery(Guid OriginalPostId) : IQuery<int>;

public record ShareDto(Guid Id, Guid UserId, Guid PostId, Guid OriginalPostId, int ShareTo, DateTime? CreatedAt);

public class ShareHandlers(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<CreateShareCommand, ShareDto>,
      IQueryHandler<GetSharesCountQuery, int>
{
    public async Task<ShareDto> Handle(CreateShareCommand c, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;

        var original = await db.Posts.FirstOrDefaultAsync(p => p.Id == c.OriginalPostId && !p.IsDeleted, ct)
            ?? throw new BuildingBlocks.Exceptions.NotFoundException("Bài viết gốc không tồn tại");

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Content = c.Caption ?? string.Empty,
            MediaUrl = string.Empty,
            Privacy = original.Privacy,
            CategoryId = original.CategoryId,
        };
        db.Posts.Add(newPost);

        var share = new Share
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PostId = newPost.Id,
            OriginalPostId = original.Id,
            ShareTo = (ShareTo)c.ShareTo,
        };
        db.Shares.Add(share);

        await db.SaveChangesAsync(ct);
        return new ShareDto(share.Id, share.UserId, share.PostId, share.OriginalPostId, (int)share.ShareTo, newPost.CreatedAt);
    }

    public async Task<int> Handle(GetSharesCountQuery q, CancellationToken ct) =>
        await db.Shares.CountAsync(s => s.OriginalPostId == q.OriginalPostId, ct);
}

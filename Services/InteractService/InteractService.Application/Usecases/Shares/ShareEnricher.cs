using InteractService.Application.DTOs.Post.Responses;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.Shares;

/// <summary>
/// Enrich danh sách PostResponseDto đã map sẵn với thông tin chia sẻ (IsShared/OriginalPost/SharesCount),
/// gọi ở tầng Handler (không sửa các query cursor phức tạp trong PostRepository) — theo dõi bảng Shares.
/// </summary>
public static class ShareEnricher
{
    public static async Task EnrichAsync(IApplicationDbContext db, IEnumerable<PostResponseDto> postsSource, CancellationToken ct)
    {
        var posts = postsSource.ToList();
        if (posts.Count == 0) return;
        var postIds = posts.Select(p => p.Id).ToList();

        // 1) Bài nào trong danh sách LÀ một lượt share (map PostId -> OriginalPostId)
        var sharesByPostId = await db.Shares
            .Where(s => postIds.Contains(s.PostId))
            .ToDictionaryAsync(s => s.PostId, s => s.OriginalPostId, ct);

        // 2) Với các bài là share, lấy nội dung bài gốc để hiển thị (kể cả khi bài gốc không nằm trong trang hiện tại)
        var originalIds = sharesByPostId.Values.Distinct().ToList();
        var originals = originalIds.Count == 0
            ? new List<Domain.Models.Post>()
            : await db.Posts.Where(p => originalIds.Contains(p.Id) && !p.IsDeleted).ToListAsync(ct);
        var originalById = originals.ToDictionary(p => p.Id);

        // 3) Số lượt bài gốc (trong danh sách hiện tại) đã được chia sẻ
        var sharesCountByOriginal = await db.Shares
            .Where(s => postIds.Contains(s.OriginalPostId))
            .GroupBy(s => s.OriginalPostId)
            .Select(g => new { OriginalPostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OriginalPostId, x => x.Count, ct);

        foreach (var dto in posts)
        {
            if (sharesCountByOriginal.TryGetValue(dto.Id, out var sc)) dto.SharesCount = sc;

            if (!sharesByPostId.TryGetValue(dto.Id, out var originalPostId)) continue;
            dto.IsShared = true;
            dto.OriginalPostId = originalPostId;
            if (originalById.TryGetValue(originalPostId, out var original))
            {
                dto.OriginalPost = new PostResponseDto
                {
                    Id = original.Id,
                    Content = original.Content,
                    MediaUrl = original.MediaUrl,
                    Privacy = original.Privacy,
                    GroupId = original.GroupId,
                    CategoryId = original.CategoryId,
                    VotesCount = original.VotesCount,
                    CommentsCount = original.CommentsCount,
                    CreatedAt = (DateTimeOffset)(original.CreatedAt ?? DateTime.UtcNow),
                };
            }
        }
    }
}

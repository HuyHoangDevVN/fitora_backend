using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Services.IServices;

namespace InteractService.Application.Usecases.Blocks;

/// <summary>
/// Lọc bỏ bài viết của user/group đã bị chặn khỏi kết quả feed — mục 13.7/26.8 Claude.md.
/// Bắt buộc thực hiện ở backend (không thể chỉ lọc ở FE). Gọi ở tầng Handler, sau khi
/// PostRepository đã trả data và trước khi trả PaginatedCursorResult, theo cùng pattern
/// với ShareEnricher (tránh sửa các query cursor phức tạp trong PostRepository).
/// </summary>
public static class BlockFilter
{
    public static async Task<List<PostResponseDto>> ApplyAsync(
        IBlockedIdsProvider blockedIdsProvider,
        Guid currentUserId,
        IEnumerable<PostResponseDto> postsSource,
        CancellationToken ct)
    {
        var posts = postsSource.ToList();
        if (posts.Count == 0) return posts;

        var (blockedUserIds, blockedGroupIds) = await blockedIdsProvider.GetBlockedIdsAsync(currentUserId, ct);

        if (blockedUserIds.Count == 0 && blockedGroupIds.Count == 0) return posts;

        return posts
            .Where(p => p.User == null || !blockedUserIds.Contains(p.User.Id))
            .Where(p => !p.GroupId.HasValue || !blockedGroupIds.Contains(p.GroupId.Value))
            .ToList();
    }
}

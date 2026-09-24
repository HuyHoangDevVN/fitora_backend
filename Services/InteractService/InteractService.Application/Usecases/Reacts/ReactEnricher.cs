using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.DTOs.React.Responses;
using InteractService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.Reacts;

public static class ReactEnricher
{
    public static async Task EnrichAsync(
        IApplicationDbContext db,
        IEnumerable<PostResponseDto> postsSource,
        Guid currentUserId,
        CancellationToken ct)
    {
        var posts = postsSource.ToList();
        if (posts.Count == 0) return;

        var postIds = posts.Select(p => p.Id).ToList();

        var reacts = await db.Reacts
            .Where(r => postIds.Contains(r.TargetId) && r.TargetType == TargetType.Post)
            .ToListAsync(ct);

        var byTarget = reacts.GroupBy(r => r.TargetId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var dto in posts)
        {
            if (!byTarget.TryGetValue(dto.Id, out var list) || list.Count == 0)
            {
                dto.ReactSummary = new ReactSummaryDto
                {
                    TargetId = dto.Id,
                    CountsByType = new Dictionary<string, int>(),
                    MyReaction = null
                };
                continue;
            }

            dto.ReactSummary = new ReactSummaryDto
            {
                TargetId = dto.Id,
                CountsByType = list.GroupBy(r => r.ReactType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                MyReaction = list.FirstOrDefault(r => r.UserId == currentUserId)?.ReactType.ToString()
            };
        }
    }
}

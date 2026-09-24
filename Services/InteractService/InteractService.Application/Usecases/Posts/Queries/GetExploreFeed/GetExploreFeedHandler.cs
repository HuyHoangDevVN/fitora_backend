using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetExploreFeed;

public class GetExploreFeedHandler (IPostRepository postRepo, IApplicationDbContext db, IBlockedIdsProvider blockedIdsProvider) : IQueryHandler<GetExploreFeedQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetExploreFeedQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetExploreFeed(query.Request);
        await ShareEnricher.EnrichAsync(db, posts.Data, cancellationToken);
        var filtered = await BlockFilter.ApplyAsync(blockedIdsProvider, query.Request.Id, posts.Data, cancellationToken);
        await ReactEnricher.EnrichAsync(db, filtered, query.Request.Id, cancellationToken);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: posts.Cursor,
            limit: posts.Limit,
            count: posts.Count,
            data: filtered,
            nextCursor: posts.NextCursor
        );
    }
}
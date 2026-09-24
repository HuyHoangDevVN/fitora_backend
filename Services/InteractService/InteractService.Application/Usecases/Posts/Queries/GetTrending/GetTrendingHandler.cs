using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetTrending;

public class GetTrendingHandler(IPostRepository postRepo, ICategoryRepository categoryRepo, IMapper mapper, IApplicationDbContext db, IBlockedIdsProvider blockedIdsProvider)
    : IQueryHandler<GetTrendingQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetTrendingQuery query,
        CancellationToken cancellationToken)
    {
        var trendingCategories = await categoryRepo.GetTrendingCategories(10, TimeSpan.FromDays(7));
        var trendingPosts = await postRepo.GetTrendingFeed(query.Request, trendingCategories);
        await ShareEnricher.EnrichAsync(db, trendingPosts.Data, cancellationToken);
        var filtered = await BlockFilter.ApplyAsync(blockedIdsProvider, query.Request.Id, trendingPosts.Data, cancellationToken);
        await ReactEnricher.EnrichAsync(db, filtered, query.Request.Id, cancellationToken);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: trendingPosts.Cursor,
            limit: trendingPosts.Limit,
            count: trendingPosts.Count,
            data: filtered,
            nextCursor: trendingPosts.NextCursor
        );
    }
}
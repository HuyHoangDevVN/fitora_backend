using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetNewfeed;

public class GetNewfeedQueryHandler(IPostRepository postRepo, IMapper mapper, IApplicationDbContext db, IBlockedIdsProvider blockedIdsProvider)
    : IQueryHandler<GetNewfeedQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetNewfeedQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetNewfeed(query.Request);
        await ShareEnricher.EnrichAsync(db, posts.Data, cancellationToken);
        var filtered = await BlockFilter.ApplyAsync(blockedIdsProvider, query.Request.Id, posts.Data, cancellationToken);
        await ReactEnricher.EnrichAsync(db, filtered, query.Request.Id, cancellationToken);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: query.Request.Cursor,
            limit: query.Request.Limit,
            count: posts.Count,
            data: filtered,
            nextCursor: posts.NextCursor
        );
    }
}
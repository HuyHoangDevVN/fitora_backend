using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetPersonal;

public class GetPersonalHandler(IPostRepository postRepo, IMapper mapper, IApplicationDbContext db, IBlockedIdsProvider blockedIdsProvider)
    : IQueryHandler<GetPersonalQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetPersonalQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetPersonal(query.Request);
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
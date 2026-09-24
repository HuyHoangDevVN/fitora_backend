using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Services.IServices;
using InteractService.Application.Usecases.Blocks;
using InteractService.Application.Usecases.Reacts;
using InteractService.Application.Usecases.Shares;

namespace InteractService.Application.Usecases.Posts.Queries.GetSavedPosts;

public class GetSavedPostsHandler(IPostRepository postRepo, IApplicationDbContext db, IBlockedIdsProvider blockedIdsProvider)
    : IQueryHandler<GetSavedPostsQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetSavedPostsQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetSavedPosts(request.Request);
        await ShareEnricher.EnrichAsync(db, posts.Data, cancellationToken);
        var filtered = await BlockFilter.ApplyAsync(blockedIdsProvider, request.Request.Id, posts.Data, cancellationToken);
        await ReactEnricher.EnrichAsync(db, filtered, request.Request.Id, cancellationToken);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: posts.Cursor,
            limit: posts.Limit,
            count: posts.Count,
            data: filtered,
            nextCursor: posts.NextCursor
        );
    }
}
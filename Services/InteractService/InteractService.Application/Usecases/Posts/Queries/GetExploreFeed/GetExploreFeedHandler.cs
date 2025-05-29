using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetExploreFeed;

public class GetExploreFeedHandler (IPostRepository postRepo) : IQueryHandler<GetExploreFeedQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetExploreFeedQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetExploreFeed(query.Request);
        return posts;
    }
}
using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetSavedPosts;

public class GetSavedPostsHandler(IPostRepository postRepo)
    : IQueryHandler<GetSavedPostsQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetSavedPostsQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetSavedPosts(request.Request);
        return posts;
    }
}
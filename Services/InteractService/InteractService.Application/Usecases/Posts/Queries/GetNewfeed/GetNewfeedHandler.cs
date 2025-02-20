using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.Usecases.Posts.Queries.GetAllPost;

namespace InteractService.Application.Usecases.Posts.Queries.GetNewfeed;

public class GetNewfeedQueryHandler(IPostRepository postRepo, IMapper mapper) : IQueryHandler<GetNewfeedQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetNewfeedQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetNewfeed(query.Request);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: query.Request.Cursor,
            limit: query.Request.Limit,
            count: posts.Count,
            data: posts.Data,
            nextCursor: posts.NextCursor
        );
    }
}
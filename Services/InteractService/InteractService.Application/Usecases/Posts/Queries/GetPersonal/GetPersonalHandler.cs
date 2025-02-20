using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetPersonal;

public class GetPersonalHandler(IPostRepository postRepo, IMapper mapper)
    : IQueryHandler<GetPersonalQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetPersonalQuery query,
        CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetPersonal(query.Request);
        return new PaginatedCursorResult<PostResponseDto>(
            cursor: query.Request.Cursor,
            limit: query.Request.Limit,
            count: posts.Count,
            data: posts.Data,
            nextCursor: posts.NextCursor
        );
    }
}
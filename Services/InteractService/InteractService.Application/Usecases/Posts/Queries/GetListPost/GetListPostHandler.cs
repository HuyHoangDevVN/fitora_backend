using BuildingBlocks.Pagination.Base;

namespace InteractService.Application.Usecases.Posts.Queries.GetListPost;

public class GetListPostHandler(IPostRepository postRepo) : IQueryHandler<GetListPostQuery, PaginatedResult<PostResponseDto>>
{
    public async Task<PaginatedResult<PostResponseDto>> Handle(GetListPostQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetListPost(query.Request);
        return posts;
    }
}
using InteractService.Application.Usecases.Posts.Queries.GetAllPost;

namespace InteractService.Application.Usecases.Posts.Queries.GetNewfeed;

public class GetNewfeedQueryHandler(IPostRepository postRepo, IMapper mapper) : IQueryHandler<GetNewfeedQuery, IEnumerable<PostResponseDto>>
{
    public async Task<IEnumerable<PostResponseDto>> Handle(GetNewfeedQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetAllAsync();
        var result = mapper.Map<IEnumerable<PostResponseDto>>(posts);
        return result;
    }
}
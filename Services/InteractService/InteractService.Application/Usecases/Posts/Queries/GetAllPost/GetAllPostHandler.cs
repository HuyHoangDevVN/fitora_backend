namespace InteractService.Application.Usecases.Posts.Queries.GetAllPost;

public class GetAllPostHandler(IPostRepository postRepo, IMapper mapper) : IQueryHandler<GetAllPostQuery, IEnumerable<PostResponseDto>>
{
    public async Task<IEnumerable<PostResponseDto>> Handle(GetAllPostQuery query, CancellationToken cancellationToken)
    {
        var posts = await postRepo.GetAllAsync();
        var result = mapper.Map<IEnumerable<PostResponseDto>>(posts);
        return result;
    }
}
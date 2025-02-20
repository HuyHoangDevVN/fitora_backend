using InteractService.Application.Usecases.Posts.Queries.GetByIdPost;

namespace InteractService.Application.Usecases.Posts.Queries.GetPostById;

public class GetPostByIdHandler : IQueryHandler<GetPostByIdQuery, PostResponseDto>
{
    private readonly IPostRepository _postRepo;
    private readonly IMapper _mapper;

    public GetPostByIdHandler(IPostRepository postRepo, IMapper mapper)
    {
        _postRepo = postRepo;
        _mapper = mapper;
    }

    public async Task<PostResponseDto> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await _postRepo.GetByIdAsync(query.Id);
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        return _mapper.Map<PostResponseDto>(post);
    }
}
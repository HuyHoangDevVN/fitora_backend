namespace InteractService.Application.Usecases.Posts.Commands.CreatePost;

public class CreatePostHandler(IPostRepository postRepo,ICategoryRepository categoryRepo, IMapper mapper)
    : ICommandHandler<CreatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        var post = mapper.Map<Post>(command.Request);
        await postRepo.CreateAsync(post);
        return mapper.Map<PostResponseDto>(post);
    }
}
namespace InteractService.Application.Usecases.Posts.Commands.CreatePost;

public class CreatePostHandler(IPostRepository postRepo, IMapper mapper) : ICommandHandler<CreatePostCommand, PostResponseDto>
{
    public async Task<PostResponseDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = mapper.Map<Post>(request.Request);
        var isSuccess = await postRepo.CreateAsync(post);
        if (!isSuccess)
        {
            throw new InvalidOperationException("Failed to create post.");
        }
        return mapper.Map<PostResponseDto>(post);
    }
}
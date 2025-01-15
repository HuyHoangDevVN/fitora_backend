namespace InteractService.Application.Usecases.Posts.Commands.UpdatePost;

public class UpdatePostHandler : ICommandHandler<UpdatePostCommand, PostResponseDto>
{
    private readonly IPostRepository _postRepo;
    private readonly IMapper _mapper;

    public UpdatePostHandler(IPostRepository postRepo, IMapper mapper)
    {
        _postRepo = postRepo;
        _mapper = mapper;
    }
    
    public async Task<PostResponseDto> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var existingPost = await _postRepo.GetByIdAsync(command.Id);
        if (existingPost is null)
        {
            throw new NotFoundException("Post not found");
        }

        _mapper.Map(command.Request, existingPost);
        
        var isUpdated = await _postRepo.UpdateAsync(existingPost);
        
        if (!isUpdated)
        {
            throw new InvalidOperationException("Failed to update post");
        }
        
        return _mapper.Map<PostResponseDto>(existingPost);
    }
}
namespace InteractService.Application.Usecases.Posts.Commands.DeletePost;

public class DeletePostHandler : ICommandHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepo;

    public DeletePostHandler(IPostRepository postRepo)
    {
        _postRepo = postRepo;
    }
    
    public async Task<bool> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        return await _postRepo.DeleteAsync(command.Id);
    }
}
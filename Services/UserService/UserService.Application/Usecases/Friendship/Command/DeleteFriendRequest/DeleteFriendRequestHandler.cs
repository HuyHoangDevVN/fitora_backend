namespace UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;

public class DeleteFriendRequestHandler (IFriendshipRepository friendshipRepo, IMapper mapper) : ICommandHandler<DeleteFriendRequestCommand, bool>
{
    public async Task<bool> Handle(DeleteFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await friendshipRepo.DeleteFriendRequestAsync(request.Id);
        return isSuccess;
    }
}
namespace UserService.Application.Usecases.Friendship.Command.UnFriend;

public class UnfriendHandler (IFriendshipRepository friendshipRepo, IMapper mapper) : ICommandHandler<UnfriendCommand, bool>
{
    public async Task<bool> Handle(UnfriendCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await friendshipRepo.UnfriendAsync(request.Id);
        return isSuccess;
    }
}
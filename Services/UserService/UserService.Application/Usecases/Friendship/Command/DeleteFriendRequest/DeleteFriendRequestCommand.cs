namespace UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;

public record DeleteFriendRequestCommand(Guid Id) : ICommand<bool>;
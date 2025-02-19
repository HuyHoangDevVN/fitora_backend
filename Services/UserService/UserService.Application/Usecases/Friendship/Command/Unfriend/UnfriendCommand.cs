namespace UserService.Application.Usecases.Friendship.Command.UnFriend;

public record UnfriendCommand(Guid Id) : ICommand<bool>;
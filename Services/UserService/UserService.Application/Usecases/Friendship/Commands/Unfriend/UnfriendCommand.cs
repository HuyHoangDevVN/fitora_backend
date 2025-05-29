namespace UserService.Application.Usecases.Friendship.Commands.Unfriend;

public record UnfriendCommand(Guid Id) : ICommand<bool>;
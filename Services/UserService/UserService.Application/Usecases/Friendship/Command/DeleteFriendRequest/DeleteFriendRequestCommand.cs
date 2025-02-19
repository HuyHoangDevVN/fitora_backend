namespace UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;

public record DeleteFriendRequestCommand(DTOs.Friendship.Requests.CreateFriendRequest Request) : ICommand<bool>;
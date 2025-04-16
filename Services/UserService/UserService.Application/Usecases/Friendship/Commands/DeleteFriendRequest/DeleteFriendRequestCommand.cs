namespace UserService.Application.Usecases.Friendship.Commands.DeleteFriendRequest;

public record DeleteFriendRequestCommand(DTOs.Friendship.Requests.CreateFriendRequest Request) : ICommand<bool>;
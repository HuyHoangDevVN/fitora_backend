using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Commands.CreateFriendRequest;

public record CreateFriendRequestCommand(DTOs.Friendship.Requests.CreateFriendRequest Request) : ICommand<ResponseDto>;
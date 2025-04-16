using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Friendship.Commands.AccpectFriendRequest;

public record AcceptFriendRequestCommand(DTOs.Friendship.Requests.CreateFriendRequest request) :  ICommand<ResponseDto>;

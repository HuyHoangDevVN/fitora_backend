using BuildingBlocks.DTOs;
using BuildingBlocks.Responses.Result;

namespace UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;

public record AcceptFriendRequestCommand(DTOs.Friendship.Requests.CreateFriendRequest request) :  ICommand<ResponseDto>;

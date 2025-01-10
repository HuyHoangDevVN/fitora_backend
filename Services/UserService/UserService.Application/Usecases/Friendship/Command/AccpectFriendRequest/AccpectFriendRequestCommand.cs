using BuildingBlocks.DTOs;
using BuildingBlocks.Responses.Result;

namespace UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;

public record AcceptFriendRequestCommand(Guid Id) :  ICommand<ResponseDto>;
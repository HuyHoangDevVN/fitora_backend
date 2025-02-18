using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Follow.Requests;

namespace UserService.Application.Usecases.Follow.Command.Unfollow;

public record UnfollowCommand(FollowRequest Request) : ICommand<ResponseDto>;
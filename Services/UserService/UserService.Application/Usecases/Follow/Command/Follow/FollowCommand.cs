using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Follow.Requests;

namespace UserService.Application.Usecases.Follow.Command.Follow;

public record FollowCommand(FollowRequest Request) : ICommand<ResponseDto>;
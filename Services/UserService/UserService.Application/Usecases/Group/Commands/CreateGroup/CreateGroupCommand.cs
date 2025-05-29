using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Group.Requests;

namespace UserService.Application.Usecases.Group.Commands.CreateGroup;

public record CreateGroupCommand(CreateGroupRequest Request) : ICommand<ResponseDto>;
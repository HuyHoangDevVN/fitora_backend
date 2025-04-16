using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Group.Requests;

namespace UserService.Application.Usecases.Group.Commands.UpdateGroup;

public record UpdateGroupCommand(UpdateGroupRequest Request) : ICommand<ResponseDto>;
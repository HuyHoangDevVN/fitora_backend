using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.DeleteGroup;

public record DeleteGroupCommand(Guid Id) : ICommand<ResponseDto>;
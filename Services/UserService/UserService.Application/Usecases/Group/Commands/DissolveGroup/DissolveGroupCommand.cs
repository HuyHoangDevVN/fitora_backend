using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.DissolveGroup;

public record DissolveGroupCommand(Guid GroupId) : ICommand<ResponseDto>;

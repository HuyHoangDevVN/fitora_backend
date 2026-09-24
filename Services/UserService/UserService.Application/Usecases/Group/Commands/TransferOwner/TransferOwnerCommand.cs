using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.TransferOwner;

public record TransferOwnerCommand(Guid GroupId, Guid NewOwnerId) : ICommand<ResponseDto>;

using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Blocks.Commands.UnblockGroup;

public record UnblockGroupCommand(Guid BlockerId, Guid GroupId) : ICommand<ResponseDto>;

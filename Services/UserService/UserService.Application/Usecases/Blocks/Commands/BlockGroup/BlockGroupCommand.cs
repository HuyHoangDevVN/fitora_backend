using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Blocks.Commands.BlockGroup;

public record BlockGroupCommand(Guid BlockerId, Guid GroupId) : ICommand<ResponseDto>;

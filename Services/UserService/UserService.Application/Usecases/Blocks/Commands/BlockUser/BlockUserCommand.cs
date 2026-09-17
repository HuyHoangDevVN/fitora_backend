using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Blocks.Commands.BlockUser;

public record BlockUserCommand(Guid BlockerId, Guid BlockedUserId) : ICommand<ResponseDto>;

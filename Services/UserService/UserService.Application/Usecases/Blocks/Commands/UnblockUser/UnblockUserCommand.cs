using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Blocks.Commands.UnblockUser;

public record UnblockUserCommand(Guid BlockerId, Guid BlockedUserId) : ICommand<ResponseDto>;

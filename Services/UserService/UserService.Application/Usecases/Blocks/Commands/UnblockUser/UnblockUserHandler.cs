using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Commands.UnblockUser;

public class UnblockUserHandler(IBlockRepository blockRepository) : ICommandHandler<UnblockUserCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
        => blockRepository.UnblockUserAsync(request.BlockerId, request.BlockedUserId);
}

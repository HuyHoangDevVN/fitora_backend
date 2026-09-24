using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Commands.BlockUser;

public class BlockUserHandler(IBlockRepository blockRepository) : ICommandHandler<BlockUserCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        => blockRepository.BlockUserAsync(request.BlockerId, request.BlockedUserId);
}

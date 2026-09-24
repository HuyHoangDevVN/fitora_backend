using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Commands.BlockGroup;

public class BlockGroupHandler(IBlockRepository blockRepository) : ICommandHandler<BlockGroupCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(BlockGroupCommand request, CancellationToken cancellationToken)
        => blockRepository.BlockGroupAsync(request.BlockerId, request.GroupId);
}

using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Commands.UnblockGroup;

public class UnblockGroupHandler(IBlockRepository blockRepository) : ICommandHandler<UnblockGroupCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(UnblockGroupCommand request, CancellationToken cancellationToken)
        => blockRepository.UnblockGroupAsync(request.BlockerId, request.GroupId);
}

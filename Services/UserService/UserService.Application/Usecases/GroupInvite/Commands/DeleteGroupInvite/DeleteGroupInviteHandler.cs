using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.DeleteGroupInvite;

public class DeleteGroupInviteHandler(IGroupInviteRepository groupInviteRepo)
    : ICommandHandler<DeleteGroupInviteCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupInviteCommand command, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.DeleteAsync(command.Id);
        return new ResponseDto(result);
    }
}
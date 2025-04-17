using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.AcceptGroupInvite;

public class AcceptGroupInviteHandler(IGroupInviteRepository groupInviteRepo, IMapper mapper)
    : ICommandHandler<AcceptGroupInviteCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(AcceptGroupInviteCommand command, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.AcceptAsync(command.Id);
        return new ResponseDto(result);
    }
}
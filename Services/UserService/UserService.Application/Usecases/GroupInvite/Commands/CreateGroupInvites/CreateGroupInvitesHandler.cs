using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;

public class CreateGroupInvitesHandler(IGroupInviteRepository groupInviteRepo, IMapper mapper)
    : ICommandHandler<CreateGroupInvitesCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupInvitesCommand command, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.CreateRangeAsync(command.Request);
        return result;
    }
}
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvite;

public class CreateGroupInviteHandler(IGroupInviteRepository groupInviteRepo, IMapper mapper)
    : ICommandHandler<CreateGroupInviteCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupInviteCommand command, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.CreateAsync(command.Request);
        return result;
    }
}
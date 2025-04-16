using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;

public class CreateGroupInvitesHandler(IGroupInviteRepository groupInviteRepo, IMapper mapper)
    : ICommandHandler<CreateGroupInvitesCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupInvitesCommand request, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.CreateRangeAsync(request.Request);
        return result;
    }
}
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupMember.Commands.AssignRoleMember;

public class AssignRoleMemberHandler(IGroupMemberRepository groupMemberRepo, IMapper mapper) : ICommandHandler<AssignRoleMemberCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(AssignRoleMemberCommand command, CancellationToken cancellationToken)
    {
        var result = await groupMemberRepo.AssignRoleAsync(command.Request);
        return (result);
    }
}
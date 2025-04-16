using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Commands.CreateMember;

public class CreateMemberHandler(IGroupMemberRepository groupMemberRepo, IMapper mapper)
    : ICommandHandler<CreateMemberCommand, GroupMemberDto>
{
    public async Task<GroupMemberDto> Handle(CreateMemberCommand command, CancellationToken cancellationToken)
    {
        var result = await groupMemberRepo.CreateAsync(mapper.Map<Domain.Models.GroupMember>(command.Request));
        return result;
    }
}
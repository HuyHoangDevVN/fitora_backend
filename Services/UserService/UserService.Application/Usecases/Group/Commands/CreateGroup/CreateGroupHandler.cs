using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.CreateGroup;

public class CreateGroupHandler(IGroupRepository groupRepo, IGroupMemberRepository groupMemberRepo, IMapper mapper)
    : ICommandHandler<CreateGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var group = mapper.Map<Domain.Models.Group>(command.Request);
        var isSucess = await groupRepo.CreateAsync(group);
        var groupMember = new Domain.Models.GroupMember
        {
            GroupId = group.Id,
            UserId = command.Request.UserId,
            Role = Domain.Enums.GroupRole.Owner
        };
        var groupMemberDto = isSucess ? await groupMemberRepo.CreateAsync(groupMember) : null;
        return new ResponseDto(
            groupMemberDto, isSucess, isSucess ? "Tạo nhóm thành công!" : "Tạo nhóm thất bại!"
        );
    }
}
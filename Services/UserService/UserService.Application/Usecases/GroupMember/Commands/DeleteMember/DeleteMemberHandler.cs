using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupMember.Commands.DeleteMember;

public class DeleteMemberHandler (IGroupMemberRepository groupMemberRepo) : ICommandHandler<DeleteMemberCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteMemberCommand command, CancellationToken cancellationToken)
    {
        var isSucces = await groupMemberRepo.DeleteAsync(command.MemberId, command.RequestedBy);
        return new ResponseDto(isSucces);
    }
}
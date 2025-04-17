using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Queries.GetById;

public class GetMemberByIdHandler (IGroupMemberRepository groupMemberRepo) : IQueryHandler<GetMemberByIdQuery, GroupMemberDto>
{
    public async Task<GroupMemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await groupMemberRepo.GetByIdAsync(request.Id, request.GroupId);
        return member;
    }
}
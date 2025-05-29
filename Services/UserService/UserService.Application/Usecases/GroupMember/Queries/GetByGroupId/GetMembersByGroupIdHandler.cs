using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Queries.GetByGroupId;

public class GetMembersByGroupIdHandler (IGroupMemberRepository groupMemberRepo, IMapper mapper) : IQueryHandler<GetMembersByGroupIdQuery, PaginatedResult<GroupMemberDto>>
{
    public async Task<PaginatedResult<GroupMemberDto>> Handle(GetMembersByGroupIdQuery request, CancellationToken cancellationToken)
    {
        var groupMembers = await groupMemberRepo.GetByGroupIdAsync(request.Request);
        return groupMembers;
    }
}
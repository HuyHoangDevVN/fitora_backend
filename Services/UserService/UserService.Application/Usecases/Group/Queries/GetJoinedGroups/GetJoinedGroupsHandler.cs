using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Responses;
using UserService.Application.Usecases.Group.Queries.GetManagedGroups;

namespace UserService.Application.Usecases.Group.Queries.GetJoinedGroups;

public class GetJoinedGroupsHandler(IGroupRepository groupRepo, IMapper mapper)
    : IQueryHandler<GetJoinedGroupsQuery, PaginatedResult<GroupDto>>
{
    public async Task<PaginatedResult<GroupDto>> Handle(GetJoinedGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await groupRepo.GetJoinedGroupsAsync(query.Request);
        var groupDtos = mapper.Map<PaginatedResult<GroupDto>>(groups);
        return groupDtos;
    }
}
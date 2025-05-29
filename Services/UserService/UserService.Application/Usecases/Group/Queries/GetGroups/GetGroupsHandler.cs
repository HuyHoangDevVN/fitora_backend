using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetGroups;

public class GetGroupsHandler(IGroupRepository groupRepo, IMapper mapper)
    : IQueryHandler<GetGroupsQuery, PaginatedResult<GroupDto>>
{
    public async Task<PaginatedResult<GroupDto>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await groupRepo.GetGroupsAsync(query.Request);
        var groupDtos = mapper.Map<PaginatedResult<GroupDto>>(groups);
        return groupDtos;
    }
}
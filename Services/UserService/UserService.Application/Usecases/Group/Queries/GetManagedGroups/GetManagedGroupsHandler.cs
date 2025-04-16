using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetManagedGroups;

public class GetManagedGroupsHandler (IGroupRepository groupRepo, IMapper mapper) : IQueryHandler<GetManagedGroupsQuery ,PaginatedResult<GroupDto>>
{
    public async Task<PaginatedResult<GroupDto>> Handle(GetManagedGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await groupRepo.GetManagedGroupsAsync(query.Request);
        var groupDtos = mapper.Map<PaginatedResult<GroupDto>>(groups);
        return groupDtos;
    }
}
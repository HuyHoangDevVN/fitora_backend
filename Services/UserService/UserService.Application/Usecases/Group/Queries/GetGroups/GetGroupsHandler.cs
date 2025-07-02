using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetGroups;

public class GetGroupsHandler(IGroupRepository groupRepo, IMapper mapper)
    : IQueryHandler<GetGroupsQuery, PaginatedResult<Domain.Models.Group>>
{
    public async Task<PaginatedResult<Domain.Models.Group>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await groupRepo.GetGroupsAsync(query.Request);
        return groups;
    }
}
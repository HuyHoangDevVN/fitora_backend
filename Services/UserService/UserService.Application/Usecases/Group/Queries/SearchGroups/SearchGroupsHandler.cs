using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.Group.Queries.SearchGroups;

public class SearchGroupsHandler(IGroupRepository groupRepo)
    : IQueryHandler<SearchGroupsQuery, PaginatedResult<Domain.Models.Group>>
{
    public async Task<PaginatedResult<Domain.Models.Group>> Handle(
        SearchGroupsQuery query, CancellationToken cancellationToken)
    {
        return await groupRepo.SearchGroupsAsync(query.Request, query.CurrentUserId);
    }
}

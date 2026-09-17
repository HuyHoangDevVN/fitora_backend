using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.Blocks.Queries.GetBlockedGroups;

public record GetBlockedGroupsQuery(Guid BlockerId, int PageIndex = 0, int PageSize = 20)
    : IQuery<PaginatedResult<Domain.Models.BlockedGroup>>;

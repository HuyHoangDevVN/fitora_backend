using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.Blocks.Queries.GetBlockedUsers;

public record GetBlockedUsersQuery(Guid BlockerId, int PageIndex = 0, int PageSize = 20)
    : IQuery<PaginatedResult<Domain.Models.Block>>;

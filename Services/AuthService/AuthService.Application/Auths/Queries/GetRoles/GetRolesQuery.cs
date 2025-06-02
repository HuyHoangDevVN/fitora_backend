using BuildingBlocks.Pagination.Base;

namespace AuthService.Application.Auths.Queries.GetRoles;

public record GetRolesQuery(int PageIndex = 0, int PageSize = 10) : IQuery<PaginatedResult<Object>>;

public record GetRolesResult(IEnumerable<object> Response);

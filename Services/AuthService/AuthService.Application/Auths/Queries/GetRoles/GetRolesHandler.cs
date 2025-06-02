using BuildingBlocks.Pagination.Base;

namespace AuthService.Application.Auths.Queries.GetRoles;

public class GetRolesHandler
(IRoleRepository roleRepository)
: IQueryHandler<GetRolesQuery, PaginatedResult<Object>>
{
    public async Task<PaginatedResult<Object>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await roleRepository.GetRolesAsync(new PaginationRequest(request.PageIndex, request.PageSize),cancellationToken);
        return (result);
    }
}
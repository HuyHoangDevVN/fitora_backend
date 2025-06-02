namespace AuthService.Application.Auths.Queries.GetRole;

public class GetRoleHandler(IRoleRepository roleRepository)
    : IQueryHandler<GetRoleQuery, object>
{
    public async Task<object> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        var result = await roleRepository.GetRoleByIdAsync(query.Id, cancellationToken);
        return result;
    }
}
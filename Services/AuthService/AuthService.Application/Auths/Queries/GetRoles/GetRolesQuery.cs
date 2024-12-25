namespace AuthService.Application.Auths.Queries.GetRoles;

public record GetRolesQuery() : IQuery<GetRolesResult>;

public record GetRolesResult(IEnumerable<object> Response);

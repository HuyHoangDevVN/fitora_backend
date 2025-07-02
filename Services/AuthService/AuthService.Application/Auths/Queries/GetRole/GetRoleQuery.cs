namespace AuthService.Application.Auths.Queries.GetRole;

public record GetRoleQuery(string Id) : IQuery<object>;
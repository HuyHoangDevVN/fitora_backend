namespace AuthService.Application.Auths.Commands.RemoveRoles;

public record RemoveRolesCommand(string[] RoleNames, string Email) : ICommand<RemoveRolesResult>;
public record RemoveRolesResult(bool IsSuccess);
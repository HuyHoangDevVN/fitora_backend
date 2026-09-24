namespace AuthService.Application.Auths.Commands.AuthDeleteAccount;

public record AuthDeleteAccountCommand(string UserId, string Password) : ICommand<AuthDeleteAccountResult>;

public record AuthDeleteAccountResult(bool IsSuccess);
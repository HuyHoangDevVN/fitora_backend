namespace AuthService.Application.Auths.Commands.AuthLockAccount;

public record AuthLockAccountCommand(int Expire) : ICommand<AuthLockAccountResult>;

public record AuthLockAccountResult(bool IsSuccess);
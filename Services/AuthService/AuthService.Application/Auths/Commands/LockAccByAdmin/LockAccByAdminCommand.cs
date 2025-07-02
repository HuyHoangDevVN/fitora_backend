using AuthService.Application.Auths.Commands.AuthLockAccount;

namespace AuthService.Application.Auths.Commands.LockAccByAdmin;

public record LockAccByAdminCommand(Guid Id) : ICommand<AuthLockAccountResult>;
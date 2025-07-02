using AuthService.Application.Auths.Commands.AuthLockAccount;

namespace AuthService.Application.Auths.Commands.UnlockAccByAdmin;

public record UnlockAccByAdminCommand(Guid Id) : ICommand<AuthLockAccountResult>;
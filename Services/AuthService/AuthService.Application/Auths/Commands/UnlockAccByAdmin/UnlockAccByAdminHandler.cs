using AuthService.Application.Auths.Commands.AuthLockAccount;

namespace AuthService.Application.Auths.Commands.UnlockAccByAdmin;

public class UnlockAccByAdminHandler(IAuthRepository authRepo) : ICommandHandler<UnlockAccByAdminCommand, AuthLockAccountResult>
{
    public  async Task<AuthLockAccountResult> Handle(UnlockAccByAdminCommand command, CancellationToken cancellationToken)
    {
        var result = await authRepo.UnlockUserByAdminAsync(command.Id);
        return new AuthLockAccountResult(result);
    }
} 
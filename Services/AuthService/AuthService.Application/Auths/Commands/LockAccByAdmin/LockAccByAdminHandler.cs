using AuthService.Application.Auths.Commands.AuthLockAccount;

namespace AuthService.Application.Auths.Commands.LockAccByAdmin;

public class LockAccByAdminHandler(IAuthRepository authRepo) : ICommandHandler<LockAccByAdminCommand, AuthLockAccountResult>
{
    public  async Task<AuthLockAccountResult> Handle(LockAccByAdminCommand command, CancellationToken cancellationToken)
    {
        var result = await authRepo.LockUserByAdminAsync(command.Id);
        return new AuthLockAccountResult(result);
    }
}
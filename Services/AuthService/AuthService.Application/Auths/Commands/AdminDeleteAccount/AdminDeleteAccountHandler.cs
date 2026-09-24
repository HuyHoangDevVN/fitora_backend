namespace AuthService.Application.Auths.Commands.AdminDeleteAccount;

public class AdminDeleteAccountHandler
(IAuthRepository authRepository)
: ICommandHandler<AdminDeleteAccountCommand, AdminDeleteAccountResult>
{
    public async Task<AdminDeleteAccountResult> Handle(AdminDeleteAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await authRepository.DeleteUserByAdminAsync(Guid.Parse(command.UserId));
        return new AdminDeleteAccountResult(result);
    }
}

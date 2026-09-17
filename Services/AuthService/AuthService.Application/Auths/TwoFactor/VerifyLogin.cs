using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record VerifyLoginCommand(string UserId, string Code) : ICommand<VerifyLoginResult>;
public record VerifyLoginResult(bool IsSuccess, string Message);

public class VerifyLoginHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    : ICommandHandler<VerifyLoginCommand, VerifyLoginResult>
{
    public async Task<VerifyLoginResult> Handle(VerifyLoginCommand cmd, CancellationToken ct)
    {
        var userId = cmd.UserId;
        var code = cmd.Code?.Trim() ?? "";
        // try recovery code
        var hash = TotpHelper.HashCode(code);
        var rc = await db.RecoveryCodes.FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == hash && !x.IsUsed, ct);
        if (rc != null)
        {
            rc.IsUsed = true;
            await db.SaveChangesAsync(ct);
            return new(true, "Recovery code accepted");
        }
        var secret = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId && x.IsVerified, ct);
        if (secret == null) return new(false, "2FA not enabled for user");
        if (TotpHelper.Verify(secret.SecretKey, code)) return new(true, "Verified");
        return new(false, "Invalid code");
    }
}

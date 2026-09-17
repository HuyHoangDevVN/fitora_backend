using BuildingBlocks.CQRS;
using BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record VerifySetupCommand(string Code) : ICommand<VerifySetupResult>;
public record VerifySetupResult(bool IsSuccess, string Message, List<string>? RecoveryCodes);

public class VerifySetupHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<VerifySetupCommand, VerifySetupResult>
{
    public async Task<VerifySetupResult> Handle(VerifySetupCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var secret = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (secret == null) return new(false, "No setup found. Call setup first.", null);
        if (!TotpHelper.Verify(secret.SecretKey, cmd.Code))
            return new(false, "Invalid code", null);

        secret.IsVerified = true;
        secret.VerifiedAt = DateTime.UtcNow;

        // generate recovery codes
        var codes = TotpHelper.GenerateRecoveryCodes(10);
        // remove old
        var old = await db.RecoveryCodes.Where(x => x.UserId == userId).ToListAsync(ct);
        db.RecoveryCodes.RemoveRange(old);
        foreach (var c in codes)
            db.RecoveryCodes.Add(new RecoveryCode { Id = Guid.NewGuid(), UserId = userId, CodeHash = TotpHelper.HashCode(c), CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        return new(true, "Verified", codes);
    }
}

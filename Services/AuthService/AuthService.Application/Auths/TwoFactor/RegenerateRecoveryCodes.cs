using BuildingBlocks.CQRS;
using BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record RegenerateRecoveryCodesCommand : ICommand<RegenerateRecoveryCodesResult>;
public record RegenerateRecoveryCodesResult(List<string> Codes);

public class RegenerateRecoveryCodesHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<RegenerateRecoveryCodesCommand, RegenerateRecoveryCodesResult>
{
    public async Task<RegenerateRecoveryCodesResult> Handle(RegenerateRecoveryCodesCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var s = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId && x.IsVerified, ct);
        if (s == null) throw new Exception("2FA not enabled");
        var old = await db.RecoveryCodes.Where(x => x.UserId == userId).ToListAsync(ct);
        db.RecoveryCodes.RemoveRange(old);
        var codes = TotpHelper.GenerateRecoveryCodes(10);
        foreach (var c in codes)
            db.RecoveryCodes.Add(new RecoveryCode { Id = Guid.NewGuid(), UserId = userId, CodeHash = TotpHelper.HashCode(c), CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        return new(codes);
    }
}

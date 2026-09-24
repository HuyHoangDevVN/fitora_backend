using BuildingBlocks.CQRS;
using BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record EnableTwoFactorCommand : ICommand<EnableTwoFactorResult>;
public record EnableTwoFactorResult(bool IsSuccess, string Message);
public record DisableTwoFactorCommand : ICommand<DisableTwoFactorResult>;
public record DisableTwoFactorResult(bool IsSuccess, string Message);

public class EnableTwoFactorHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResult>
{
    public async Task<EnableTwoFactorResult> Handle(EnableTwoFactorCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var s = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (s == null || !s.IsVerified) return new(false, "Not verified. Complete setup first.");
        return new(true, "2FA enabled");
    }
}

public class DisableTwoFactorHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<DisableTwoFactorCommand, DisableTwoFactorResult>
{
    public async Task<DisableTwoFactorResult> Handle(DisableTwoFactorCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var s = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (s != null) db.TotpSecrets.Remove(s);
        var codes = await db.RecoveryCodes.Where(x => x.UserId == userId).ToListAsync(ct);
        db.RecoveryCodes.RemoveRange(codes);
        await db.SaveChangesAsync(ct);
        return new(true, "2FA disabled");
    }
}

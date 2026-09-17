using BuildingBlocks.Security;

namespace AuthService.Application.Auths.TwoFactor;

public record GetTwoFactorStatusQuery : IQuery<GetTwoFactorStatusResult>;
public record GetTwoFactorStatusResult(bool IsEnabled, bool IsVerified);

public class GetTwoFactorStatusHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : IQueryHandler<GetTwoFactorStatusQuery, GetTwoFactorStatusResult>
{
    public async Task<GetTwoFactorStatusResult> Handle(GetTwoFactorStatusQuery q, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var secret = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (secret == null) return new(false, false);
        return new(true, secret.IsVerified);
    }
}

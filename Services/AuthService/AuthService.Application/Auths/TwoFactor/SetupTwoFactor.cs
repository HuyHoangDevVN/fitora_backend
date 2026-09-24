using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record SetupTwoFactorCommand : ICommand<SetupTwoFactorResult>;
public record SetupTwoFactorResult(string Secret, string QrCodeUrl);

public class SetupTwoFactorHandler(IApplicationDbContext db, IAuthorizeExtension auth, UserManager<ApplicationUser> userManager)
    : ICommandHandler<SetupTwoFactorCommand, SetupTwoFactorResult>
{
    public async Task<SetupTwoFactorResult> Handle(SetupTwoFactorCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var user = await userManager.FindByIdAsync(userId) ?? throw new Exception("User not found");
        var email = user.Email ?? user.UserName ?? userId;

        var existing = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        string secret;
        if (existing != null)
        {
            secret = existing.SecretKey;
            existing.IsVerified = false;
            existing.VerifiedAt = null;
        }
        else
        {
            secret = TotpHelper.GenerateSecret(32);
            db.TotpSecrets.Add(new TotpSecret
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SecretKey = secret,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync(ct);
        return new(secret, TotpHelper.BuildQrUrl(secret, email));
    }
}

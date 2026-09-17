using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.ForgotPassword;

public class ForgotPasswordHandler(IApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, ILogger<ForgotPasswordHandler> logger)
    : ICommandHandler<ForgotPasswordCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);

        // Always return success to avoid email enumeration; but only create token if user exists
        if (user == null)
        {
            logger.LogWarning("ForgotPassword: email not found {Email}", email);
            return new ResponseDto(Message: "If the email exists, an OTP has been sent.");
        }

        // Generate 6-digit OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var otpHash = ComputeSha256(otp);
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Invalidate previous unused tokens for this user (optional: mark expired)
        var previous = await dbContext.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
        foreach (var p in previous) p.ExpiresAt = DateTime.UtcNow; // expire old

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            OtpHash = otpHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
        };
        dbContext.PasswordResetTokens.Add(token);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Dev stub: log OTP to console (no SMTP)
        logger.LogInformation("[ForgotPassword] OTP for {Email}: {Otp} (expires {ExpiresAt:u})", email, otp, expiresAt);
        Console.WriteLine($"[ForgotPassword] OTP for {email}: {otp} (expires {expiresAt:u})");

        return new ResponseDto(Message: "If the email exists, an OTP has been sent.");
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

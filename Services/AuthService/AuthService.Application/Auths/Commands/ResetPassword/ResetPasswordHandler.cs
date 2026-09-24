using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.ResetPassword;

public class ResetPasswordHandler(IApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    : ICommandHandler<ResetPasswordCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) throw new BadRequestException("Invalid OTP");

        var hash = ComputeSha256(command.Otp.Trim());
        var token = await dbContext.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.OtpHash == hash)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (token == null) throw new BadRequestException("Invalid OTP");
        if (token.UsedAt != null) throw new BadRequestException("OTP has already been used");
        if (token.ExpiresAt < DateTime.UtcNow) throw new BadRequestException("OTP has expired");

        // Check password policy again defensively
        if (!AuthPasswordPolicy.Matches(command.NewPassword))
            throw new BadRequestException(ResetPasswordCommandValidator.PasswordPolicyMessage);

        // Reset password via Identity (generate internal token)
        var identityToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, identityToken, command.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        token.UsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResponseDto(Message: "Password has been reset successfully");
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

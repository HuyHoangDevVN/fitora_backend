using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.VerifyResetOtp;

public class VerifyResetOtpHandler(IApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    : ICommandHandler<VerifyResetOtpCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(VerifyResetOtpCommand command, CancellationToken cancellationToken)
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

        return new ResponseDto(Message: "OTP is valid");
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

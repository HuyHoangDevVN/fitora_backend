using AuthService.Application.DTOs.Auth.Responses;
using AuthService.Application.Services.IServices;
using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.TwoFactor;

public record VerifyLoginCommand(string UserId, string Code) : ICommand<VerifyLoginResult>;
public record VerifyLoginResult(bool IsSuccess, string Message, LoginResponseDto? Tokens = null);

/// <summary>
/// Bước 2 của luồng login khi user đã bật 2FA (13.9): FE gọi endpoint này sau khi
/// AuthLogin trả RequiresTwoFactor=true. Đúng OTP/recovery code mới thật sự phát
/// access/refresh token — trước đây handler chỉ trả IsSuccess mà không có token nào,
/// khiến FE không thể hoàn tất đăng nhập được.
/// </summary>
public class VerifyLoginHandler(
    IApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IAuthRepository authRepository,
    ITotpSecretProtector protector)
    : ICommandHandler<VerifyLoginCommand, VerifyLoginResult>
{
    public async Task<VerifyLoginResult> Handle(VerifyLoginCommand cmd, CancellationToken ct)
    {
        var userId = cmd.UserId;
        var code = cmd.Code?.Trim() ?? "";

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return new(false, "User not found");

        // try recovery code
        var hash = TotpHelper.HashCode(code);
        var rc = await db.RecoveryCodes.FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == hash && !x.IsUsed, ct);
        if (rc != null)
        {
            rc.IsUsed = true;
            await db.SaveChangesAsync(ct);
            var tokens = await authRepository.IssueLoginTokensAsync(user);
            return new(true, "Recovery code accepted", tokens);
        }

        var secret = await db.TotpSecrets.FirstOrDefaultAsync(x => x.UserId == userId && x.IsVerified, ct);
        if (secret == null) return new(false, "2FA not enabled for user");
        var plain = protector.Unprotect(secret.SecretKey);
        if (!TotpHelper.Verify(plain, code)) return new(false, "Invalid code");

        var loginTokens = await authRepository.IssueLoginTokensAsync(user);
        return new(true, "Verified", loginTokens);
    }
}

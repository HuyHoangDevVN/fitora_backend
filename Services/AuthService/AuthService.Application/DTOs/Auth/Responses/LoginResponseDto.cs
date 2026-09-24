namespace AuthService.Application.DTOs.Auth.Responses;

/// <summary>
/// RequiresTwoFactor=true: mật khẩu đúng nhưng user đã bật 2FA — Token=null,
/// FE phải gọi POST /auth/2fa/verify-login (kèm UserId) trước khi có access/refresh token thật (13.9).
/// </summary>
public record LoginResponseDto(
    bool IsSuccess,
    UserDto? User,
    LoginTokenResponseDto? Token,
    string? Message,
    bool RequiresTwoFactor = false);
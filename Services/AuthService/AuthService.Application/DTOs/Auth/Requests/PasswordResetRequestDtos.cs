namespace AuthService.Application.DTOs.Auth.Requests;

public record ForgotPasswordRequestDto(string Email);
public record VerifyResetOtpRequestDto(string Email, string Otp);
public record ResetPasswordRequestDto(string Email, string Otp, string NewPassword);

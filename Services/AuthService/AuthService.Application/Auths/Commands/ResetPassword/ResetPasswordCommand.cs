using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : ICommand<ResponseDto>;

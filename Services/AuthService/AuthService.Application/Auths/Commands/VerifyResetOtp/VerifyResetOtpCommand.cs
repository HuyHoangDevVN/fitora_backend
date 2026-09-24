using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.VerifyResetOtp;

public record VerifyResetOtpCommand(string Email, string Otp) : ICommand<ResponseDto>;

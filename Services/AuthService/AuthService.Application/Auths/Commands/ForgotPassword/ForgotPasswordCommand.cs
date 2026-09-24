using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand<ResponseDto>;
public record ForgotPasswordResult(bool IsSuccess, string Message);

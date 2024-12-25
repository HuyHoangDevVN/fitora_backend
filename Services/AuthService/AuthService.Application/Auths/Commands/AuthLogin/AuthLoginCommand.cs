using AuthService.Application.DTOs.Auth.Responses;
using BuildingBlocks.CQRS;

namespace AuthService.Application.Auths.Commands.AuthLogin;

public record AuthLoginCommand(string Email, string Password) : ICommand<AuthLoginResult>;

public record AuthLoginResult(LoginResponseDto ResponseDto);

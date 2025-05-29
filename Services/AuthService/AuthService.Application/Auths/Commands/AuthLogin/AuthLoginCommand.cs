using AuthService.Application.DTOs.Auth.Responses;
using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace AuthService.Application.Auths.Commands.AuthLogin;

public record AuthLoginCommand(string Email, string Password) : ICommand<LoginResponseDto>;

public record AuthLoginResult(LoginResponseDto ResponseDto);
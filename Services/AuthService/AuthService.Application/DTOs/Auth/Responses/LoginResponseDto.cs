namespace AuthService.Application.DTOs.Auth.Responses;

public record LoginResponseDto(UserDto User, LoginTokenResponseDto Token);
namespace AuthService.Application.DTOs.Auth.Responses;

public record LoginResponseDto(bool IsSuccess, UserDto? User, LoginTokenResponseDto? Token, string? Message);
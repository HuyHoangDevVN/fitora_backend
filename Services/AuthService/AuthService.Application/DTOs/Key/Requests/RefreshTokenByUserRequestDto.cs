namespace AuthService.Application.DTOs.Key.Requests;

public record RefreshTokenByUserRequestDto(string Token, Guid UserId);
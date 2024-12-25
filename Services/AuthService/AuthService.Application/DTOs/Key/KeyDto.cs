namespace AuthService.Application.DTOs.Key;

public record KeyDto(string UserId, string Token, DateTime Expire, bool IsUsed, bool IsRevoked);

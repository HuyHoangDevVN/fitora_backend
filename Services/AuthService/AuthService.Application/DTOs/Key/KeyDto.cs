namespace AuthService.Application.DTOs.Key;

// Id để FE gọi revoke; Token đã mask — không bao giờ trả refresh-token gốc ra client (26.7).
public record KeyDto(Guid Id, string UserId, string TokenHint, DateTime? CreatedAt, DateTime Expire, bool IsUsed, bool IsRevoked);

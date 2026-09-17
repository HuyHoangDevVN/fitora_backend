using BuildingBlocks.Abstractions;

namespace AuthService.Domain.Models;

public class PasswordResetToken : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string OtpHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}

namespace AuthService.Domain.Models;

using BuildingBlocks.Abstractions;

public class TotpSecret : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

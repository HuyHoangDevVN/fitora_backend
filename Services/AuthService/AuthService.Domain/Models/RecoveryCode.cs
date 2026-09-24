namespace AuthService.Domain.Models;

using BuildingBlocks.Abstractions;

public class RecoveryCode : Entity<Guid>
{
    public string UserId { get; set; } = default!;
    public string CodeHash { get; set; } = default!;
    public bool IsUsed { get; set; }
}

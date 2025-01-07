using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Tag : Entity<Guid>
{
    public Guid UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
}
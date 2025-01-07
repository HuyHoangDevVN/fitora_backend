using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class React : Entity<Guid>
{
    public Guid UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
    public ReactType ReactType { get; set; } = default!;
}
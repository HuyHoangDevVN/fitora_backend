using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class React : Entity<int>
{
    public int UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public int TargetId { get; set; }
    public ReactType ReactType { get; set; } = default!;
}
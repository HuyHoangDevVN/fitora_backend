using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Report : Entity<Guid>
{
    public Guid UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = null!;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? HanldeBy { get; set; } = null;
}
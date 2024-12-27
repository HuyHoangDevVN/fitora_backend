using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Report : Entity<int>
{
    public int UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public int TargetId { get; set; }
    public string Reason { get; set; } = null!;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public int? HanldeBy { get; set; } = null;
}
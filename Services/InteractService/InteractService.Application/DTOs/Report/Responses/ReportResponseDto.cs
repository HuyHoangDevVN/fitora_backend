using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Report.Responses;

public class ReportResponseDto
{
    public Guid UserId { get; set; }
    public TargetType TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = null!;
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public Guid? HanldeBy { get; set; } = null;
}
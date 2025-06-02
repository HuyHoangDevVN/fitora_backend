using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Report.Requests;

public record UpdateStatusReportRequest(Guid ReportId, ReportStatus Status, Guid HandledBy);
public record UpdateStatusReportFormBody(Guid ReportId, ReportStatus Status);
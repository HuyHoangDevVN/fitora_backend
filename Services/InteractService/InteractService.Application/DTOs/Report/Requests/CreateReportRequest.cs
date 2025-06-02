using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Report.Requests;

public record CreateReportRequest(
    Guid UserId,
    TargetType TargetType,
    Guid TargetId,
    string Reason
);

public record CreateReportFormBody(
    TargetType TargetType,
    Guid TargetId,
    string Reason
);

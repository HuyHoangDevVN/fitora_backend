using InteractService.Application.DTOs.Report.Responses;

namespace InteractService.Application.Usecases.Report.Queries.GetById;

public record GetReportQuery(Guid Id) : IQuery<ReportResponseDto>;
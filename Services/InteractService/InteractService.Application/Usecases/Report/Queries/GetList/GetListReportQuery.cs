using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Application.DTOs.Report.Responses;

namespace InteractService.Application.Usecases.Report.Queries.GetList;

public record GetListReportQuery(GetReportsRequest Request) : IQuery<PaginatedResult<ReportResponseDto>>;
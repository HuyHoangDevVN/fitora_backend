using BuildingBlocks.Pagination.Base;
using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Report.Requests;

public record GetReportsRequest(string? KeySearch, ReportStatus? Status, TargetType? TargetType, Guid? UserId)
    : PaginationRequest;
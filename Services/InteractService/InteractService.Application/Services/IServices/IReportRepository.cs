using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Domain.Enums;

namespace InteractService.Application.Services.IServices;

public interface IReportRepository
{
    Task<bool> AddAsync(Report report);
    Task<Report?> GetByIdAsync(Guid id);
    Task<PaginatedResult<Report>> GetListAsync(GetReportsRequest request);
    Task<bool> UpdateStatusAsync(Guid reportId, ReportStatus status, Guid handledBy);
    Task<bool> DeleteAsync(Guid reportId);
}
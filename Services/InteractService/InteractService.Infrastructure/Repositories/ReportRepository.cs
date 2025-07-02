using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Application.Services.IServices;
using InteractService.Domain.Enums;

namespace InteractService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IRepositoryBase<Report> _reportRepo;

    public ReportRepository(IRepositoryBase<Report> reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<bool> AddAsync(Report report)
    {
        report.CreatedAt = DateTime.UtcNow;
        report.CreatedBy = report.UserId;
        await _reportRepo.AddAsync(report);
        return await _reportRepo.SaveChangesAsync() > 0;
    }

    public async Task<Report?> GetByIdAsync(Guid id)
    {
        return await _reportRepo.GetAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<Report>> GetListAsync(GetReportsRequest request)
    {
        var reports = await _reportRepo.GetPageAsync(request, CancellationToken.None,
            x => (request.TargetType == null || x.TargetType == request.TargetType) &&
                 (request.UserId == null || x.UserId == request.UserId) &&
                 (string.IsNullOrWhiteSpace(request.KeySearch) ||
                  x.Reason.ToLower().Contains(request.KeySearch.ToLower())));
        return reports;
    }

    public async Task<bool> UpdateStatusAsync(Guid reportId, ReportStatus status, Guid handledBy)
    {
        var reportFound = await _reportRepo.GetAsync(x => x.Id == reportId);
        if (reportFound == null)
        {
            throw new InvalidOperationException("Report not found.");
        }
        reportFound.Status = status;
        reportFound.HanldeBy = handledBy;
        reportFound.LastModified = DateTime.UtcNow;
        await _reportRepo.UpdateAsync(r => r.Id == reportId, reportFound);
        return await _reportRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid reportId)
    {
        await _reportRepo.DeleteAsync(r => r.Id == reportId);
        return await _reportRepo.SaveChangesAsync() > 0;
    }
}
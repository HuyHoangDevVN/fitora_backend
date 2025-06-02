using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Report.Responses;

namespace InteractService.Application.Usecases.Report.Queries.GetList;

public class GetListReportHandler(IReportRepository reportRepo, IMapper mapper)
    : IQueryHandler<GetListReportQuery, PaginatedResult<ReportResponseDto>>
{
    public async Task<PaginatedResult<ReportResponseDto>> Handle(GetListReportQuery query,
        CancellationToken cancellationToken)
    {
        var reports = await reportRepo.GetListAsync(query.Request);
        var reportDtos = mapper.Map<PaginatedResult<ReportResponseDto>>(reports);
        return reportDtos;
    }
}
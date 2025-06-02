using InteractService.Application.DTOs.Report.Responses;

namespace InteractService.Application.Usecases.Report.Queries.GetById;

public class GetReportHandler(IReportRepository reportRepo, IMapper mapper)
    : IQueryHandler<GetReportQuery, ReportResponseDto>
{
    public async Task<ReportResponseDto> Handle(GetReportQuery query, CancellationToken cancellationToken)
    {
        var report = await reportRepo.GetByIdAsync(query.Id);
        return mapper.Map<ReportResponseDto>(report);
    }
}
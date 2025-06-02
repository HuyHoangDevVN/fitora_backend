using InteractService.Application.DTOs.Report.Requests;

namespace InteractService.Application.Usecases.Report.Commands.CreateReport;

public class CreateReportHandler(IReportRepository reportRepo, IMapper mapper)
    : ICommandHandler<CreateReportCommand, bool>
{
    public async Task<bool> Handle(CreateReportCommand command, CancellationToken cancellationToken)
    {
        var report = mapper.Map<Domain.Models.Report>(command.Request);
        report.Status = Domain.Enums.ReportStatus.Pending;
        var result = await reportRepo.AddAsync(report);
        return result;
    }
}
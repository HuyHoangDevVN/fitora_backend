namespace InteractService.Application.Usecases.Report.Commands.UpdateStatus;

public class UpdateStatusReportHandler(IReportRepository reportRepo, IMapper mapper)
    : ICommandHandler<UpdateStatusReportCommand, bool>
{
    public async Task<bool> Handle(UpdateStatusReportCommand command, CancellationToken cancellationToken)
    {
        var isSuccess = await reportRepo.UpdateStatusAsync(command.Request.ReportId, command.Request.Status,
            command.Request.HandledBy);
        return isSuccess;
    }
}
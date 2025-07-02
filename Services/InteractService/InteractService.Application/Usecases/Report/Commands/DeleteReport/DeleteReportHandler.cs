namespace InteractService.Application.Usecases.Report.Commands.DeleteReport;

public class DeleteReportHandler(IReportRepository reportRepo) : ICommandHandler<DeleteReportCommand, bool>
{
    public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await reportRepo.DeleteAsync(request.Id);
        return isSuccess;
    }
}
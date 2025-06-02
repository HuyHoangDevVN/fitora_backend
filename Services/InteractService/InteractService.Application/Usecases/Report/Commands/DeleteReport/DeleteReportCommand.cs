namespace InteractService.Application.Usecases.Report.Commands.DeleteReport;

public record DeleteReportCommand(Guid Id): ICommand<bool>;
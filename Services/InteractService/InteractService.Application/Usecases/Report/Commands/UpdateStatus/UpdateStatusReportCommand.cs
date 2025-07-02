using InteractService.Application.DTOs.Report.Requests;

namespace InteractService.Application.Usecases.Report.Commands.UpdateStatus;

public record UpdateStatusReportCommand(UpdateStatusReportRequest Request) : ICommand<bool>;
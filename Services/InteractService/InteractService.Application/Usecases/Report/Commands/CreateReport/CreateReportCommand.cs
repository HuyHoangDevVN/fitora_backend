using InteractService.Application.DTOs.Report.Requests;

namespace InteractService.Application.Usecases.Report.Commands.CreateReport;

public record CreateReportCommand(CreateReportRequest Request) : ICommand<bool>;
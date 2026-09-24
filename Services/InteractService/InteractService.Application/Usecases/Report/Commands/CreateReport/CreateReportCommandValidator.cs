using FluentValidation;

namespace InteractService.Application.Usecases.Report.Commands.CreateReport;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.Request.TargetId).NotEmpty();
        RuleFor(x => x.Request.Reason)
            .NotEmpty().WithMessage("Lý do báo cáo không được để trống")
            .MaximumLength(500).WithMessage("Lý do báo cáo không được vượt quá 500 ký tự");
    }
}

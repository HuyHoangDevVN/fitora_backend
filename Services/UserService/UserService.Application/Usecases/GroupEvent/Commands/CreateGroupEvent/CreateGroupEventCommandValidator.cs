using FluentValidation;

namespace UserService.Application.Usecases.GroupEvent.Commands.CreateGroupEvent;

public class CreateGroupEventCommandValidator : AbstractValidator<CreateGroupEventCommand>
{
    public CreateGroupEventCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EventDate).NotEmpty().Must(d => d > DateTime.UtcNow)
            .WithMessage("Ngày sự kiện phải ở tương lai.");
        RuleFor(x => x.Location).MaximumLength(500).When(x => x.Location is not null);
    }
}

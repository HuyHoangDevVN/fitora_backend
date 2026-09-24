using FluentValidation;

namespace UserService.Application.Usecases.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Bio).MaximumLength(1000).When(x => x.Request.Bio is not null);
        RuleFor(x => x.Request.Address).MaximumLength(500).When(x => x.Request.Address is not null);
        RuleFor(x => x.Request.PhoneNumber)
            .Matches(@"^\+?[0-9]{8,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Request.PhoneNumber))
            .WithMessage("Số điện thoại không hợp lệ.");
        RuleFor(x => x.Request.BirthDate)
            .Must(d => d <= DateTime.Today)
            .WithMessage("Ngày sinh không hợp lệ.");
    }
}

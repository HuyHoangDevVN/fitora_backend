using FluentValidation;

namespace AuthService.Application.Auths.Commands.AdminDeleteAccount;

public class AdminDeleteAccountCommandValidator : AbstractValidator<AdminDeleteAccountCommand>
{
    public AdminDeleteAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
    }
}

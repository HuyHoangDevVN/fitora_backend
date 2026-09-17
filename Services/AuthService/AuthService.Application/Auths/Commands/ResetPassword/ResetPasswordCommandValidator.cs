using FluentValidation;

namespace AuthService.Application.Auths.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public const string PasswordPolicyMessage =
        "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, a digit and a special character.";

    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Otp).NotEmpty().Length(6);
        RuleFor(x => x.NewPassword).NotEmpty().Must(AuthPasswordPolicy.Matches).WithMessage(PasswordPolicyMessage);
    }
}

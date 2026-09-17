using FluentValidation;

namespace AuthService.Application.Auths.Commands.AuthChangePassword;

public class AuthChangePasswordCommandValidator : AbstractValidator<AuthChangePasswordCommand>
{
    // Đồng bộ chính sách 4 yếu tố với FE + AuthRegisterCommandValidator (13.6.4).
    public const string PasswordPolicyMessage =
        "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, a digit and a special character.";

    public AuthChangePasswordCommandValidator()
    {
        RuleFor(c => c.OldPassword).NotEmpty().WithMessage("OldPassword is required!");
        RuleFor(c => c.NewPassword).NotEmpty().WithMessage("NewPassword is required!");
        RuleFor(c => c.NewPassword).Must(AuthPasswordPolicy.Matches).WithMessage(PasswordPolicyMessage);
        RuleFor(c => c.ConfirmPassword).NotEmpty().WithMessage("ConfirmPassword is required!");
        RuleFor(c => c.ConfirmPassword)
                .Equal(c => c.NewPassword)
                .WithMessage("ConfirmPassword must match NewPassword");
    }
}
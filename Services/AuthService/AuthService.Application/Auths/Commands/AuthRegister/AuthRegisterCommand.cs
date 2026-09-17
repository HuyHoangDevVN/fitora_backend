
using FluentValidation;

namespace AuthService.Application.Auths.Commands.AuthRegister;

public record AuthRegisterCommand(string Email, string Password, string FullName) : ICommand<AuthRegisterResult>;
public record AuthRegisterResult(LoginResponseDto LoginResponseDto);
public class AuthRegisterCommandValidator : AbstractValidator<AuthRegisterCommand>
{
    // Đồng bộ với FE src/utils/passwordPolicy.ts (13.6.4): 8+ ký tự, hoa, thường, số, ký tự đặc biệt.
    public const string PasswordPolicyMessage =
        "Password must be at least 8 characters and contain an uppercase letter, a lowercase letter, a digit and a special character.";

    public AuthRegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .NotNull()
            .WithMessage("Fullname is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Must(AuthPasswordPolicy.Matches)
            .WithMessage(PasswordPolicyMessage);
    }
}

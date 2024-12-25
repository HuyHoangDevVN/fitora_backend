using FluentValidation;

namespace AuthService.Application.Auths.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(r => r.Token).NotEmpty().WithName("Token cannot be null");
        
    }
}
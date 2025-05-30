namespace AuthService.Application.Auths.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, Guid UserId) : ICommand<RefreshTokenResult>;
public record RefreshTokenResult(string AccessToken, string RefreshToken);
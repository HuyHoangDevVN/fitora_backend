namespace AuthService.Application.Auths.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : ICommand<RefreshTokenResult>;
public record RefreshTokenResult(string AccessToken, string RefreshToken);
namespace AuthService.Application.Services.IServices;

public interface IJwtTokenGenerator
{
    string GeneratorToken(ApplicationUser user, IEnumerable<string>? roles);
    bool ValidateToken(string token);
    UserDto DecodeToken(string token);
    string GeneratorTokenByUserId(string userId);
    string GeneratorRefreshToken(string userId);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security;

public class AuthorizeExtension : IAuthorizeExtension
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtConfiguration _jwtConfiguration;

    public AuthorizeExtension(IHttpContextAccessor httpContextAccessor, IOptions<JwtConfiguration> options)
    {
        this._jwtConfiguration = options.Value ?? throw new ArgumentNullException(nameof(options));
        this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Lấy user từ token lưu trong cookie
    /// </summary>
    public UserLoginResponseBase GetUserFromClaimToken()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new BadRequestException("Invalid token");
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        return new UserLoginResponseBase
        (
            Id: Guid.Parse(userId),
            UserName: user?.FindFirst(ClaimTypes.Name)!.Value ?? "",
            FullName: user?.FindFirst("FullName")!.Value ?? ""
        );
    }

    /// <summary>
    /// Kiểm tra token có hợp lệ không (lấy từ cookie)
    /// </summary>
    public bool ValidateToken()
    {
        var token = GetToken(); // Lấy token từ cookie
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(this._jwtConfiguration.Secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = this._jwtConfiguration.Issuer,
            ValidAudience = this._jwtConfiguration.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Giải mã token lấy từ cookie
    /// </summary>
    public UserLoginResponseBase DecodeToken()
    {
        var token = GetToken(); // Lấy token từ cookie

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token))
        {
            throw new SecurityTokenException("Invalid token format");
        }

        var key = Encoding.UTF8.GetBytes(_jwtConfiguration.Secret);
        var signingKey = new SymmetricSecurityKey(key);
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtConfiguration.Issuer,
            ValidAudience = _jwtConfiguration.Audience,
            IssuerSigningKey = signingKey
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        return new UserLoginResponseBase(
            Id: Guid.Parse(userId),
            UserName: principal?.FindFirst(ClaimTypes.Name)!.Value ?? "",
            FullName: principal?.FindFirst("FullName")!.Value ?? ""
        );
    }

    /// <summary>
    /// Lấy token từ HttpOnly Cookie
    /// </summary>
    public string GetToken()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["accessToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            throw new BadRequestException("Invalid or missing Authorization token.");
        }

        return token;
    }
}

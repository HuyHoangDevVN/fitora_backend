using AuthService.Application.Data;
using BuildingBlocks.CQRS;
using BuildingBlocks.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.Commands.RevokeAllOtherKeys;

public class RevokeAllOtherKeysHandler(
    IApplicationDbContext context,
    IAuthorizeExtension authorizeExtension,
    IHttpContextAccessor httpContextAccessor) : ICommandHandler<RevokeAllOtherKeysCommand, RevokeAllOtherKeysResult>
{
    public async Task<RevokeAllOtherKeysResult> Handle(RevokeAllOtherKeysCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = authorizeExtension.DecodeToken().Id.ToString();
        // refreshToken của session hiện tại (cookie) -> giữ lại
        var currentRefreshToken = httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        var keys = await context.Keys
            .Where(k => k.UserId == currentUserId
                        && !k.IsRevoked
                        && (currentRefreshToken == null || k.Token != currentRefreshToken))
            .ToListAsync(cancellationToken);

        foreach (var k in keys)
            k.Update(expires: k.Expires, isUsed: k.IsUsed, isRevoked: true);

        if (keys.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        return new RevokeAllOtherKeysResult(keys.Count, $"Revoked {keys.Count} other session(s)");
    }
}

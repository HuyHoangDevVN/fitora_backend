using AuthService.Application.Data;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auths.Commands.RevokeKey;

public class RevokeKeyHandler(
    IApplicationDbContext context,
    IAuthorizeExtension authorizeExtension) : ICommandHandler<RevokeKeyCommand, RevokeKeyResult>
{
    public async Task<RevokeKeyResult> Handle(RevokeKeyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = authorizeExtension.DecodeToken().Id.ToString();

        var key = await context.Keys
            .FirstOrDefaultAsync(
                k => k.Id.Value == request.KeyId,
                cancellationToken);

        if (key is null)
            throw new NotFoundException($"Key {request.KeyId} not found");

        // User chỉ được revoke session của chính mình
        if (!string.Equals(key.UserId, currentUserId, StringComparison.OrdinalIgnoreCase))
            throw new UnAuthorizationException("You can only revoke your own sessions");

        key.Update(expires: key.Expires, isUsed: key.IsUsed, isRevoked: true);
        await context.SaveChangesAsync(cancellationToken);
        return new RevokeKeyResult(true, "Session revoked");
    }
}

using BuildingBlocks.CQRS;

namespace AuthService.Application.Auths.Commands.RevokeAllOtherKeys;

public record RevokeAllOtherKeysCommand : ICommand<RevokeAllOtherKeysResult>;

public record RevokeAllOtherKeysResult(int RevokedCount, string Message);

using BuildingBlocks.CQRS;

namespace AuthService.Application.Auths.Commands.RevokeKey;

public record RevokeKeyCommand(Guid KeyId) : ICommand<RevokeKeyResult>;

public record RevokeKeyResult(bool IsSuccess, string Message);

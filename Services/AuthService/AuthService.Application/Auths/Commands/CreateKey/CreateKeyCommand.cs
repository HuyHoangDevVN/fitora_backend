using AuthService.Application.DTOs.Key.Requests;

namespace AuthService.Application.Auths.Commands.CreateKey;

public record CreateKeyCommand(CreateKeyRequestDto Request) : ICommand<CreateKeyResult>;

public record CreateKeyResult(Guid Id);

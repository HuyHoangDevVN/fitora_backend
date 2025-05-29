using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.AcceptGroupInvite;

public record AcceptGroupInviteCommand(Guid Id) : ICommand<ResponseDto>;
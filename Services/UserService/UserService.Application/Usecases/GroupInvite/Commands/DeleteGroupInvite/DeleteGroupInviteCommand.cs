using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Commands.DeleteGroupInvite;

public record DeleteGroupInviteCommand(Guid Id) : ICommand<ResponseDto>;
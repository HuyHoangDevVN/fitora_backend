using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvite;

public record CreateGroupInviteCommand(CreateGroupInviteRequest Request) : ICommand<ResponseDto>;
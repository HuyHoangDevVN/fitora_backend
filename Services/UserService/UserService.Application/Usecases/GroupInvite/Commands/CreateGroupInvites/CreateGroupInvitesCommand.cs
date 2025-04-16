using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;

public record CreateGroupInvitesCommand(CreateGroupInvitesRequest Request) : ICommand<ResponseDto>;
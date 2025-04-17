using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupMember.Requests;

namespace UserService.Application.Usecases.GroupMember.Commands.AssignRoleMember;

public record AssignRoleMemberCommand(AssignRoleGroupMemberRequest Request) : ICommand<ResponseDto>;
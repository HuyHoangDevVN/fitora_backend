using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Commands.CreateMember;

public record CreateMemberCommand(CreateGroupMemberRequest Request) : ICommand<GroupMemberDto>;
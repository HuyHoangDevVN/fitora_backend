using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupMember.Commands.LeaveGroup;

public record LeaveGroupCommand(Guid GroupId) : ICommand<ResponseDto>;

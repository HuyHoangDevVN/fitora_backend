using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupMember.Commands.JoinGroup;

public record JoinGroupCommand(Guid GroupId) : ICommand<ResponseDto>;

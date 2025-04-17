using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupMember.Commands.DeleteMember;

public record DeleteMemberCommand(Guid MemberId, Guid RequestedBy): ICommand<ResponseDto>;
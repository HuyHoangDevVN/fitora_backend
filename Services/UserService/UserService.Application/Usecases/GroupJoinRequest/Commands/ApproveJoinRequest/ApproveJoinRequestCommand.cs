using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupJoinRequest.Commands.ApproveJoinRequest;

public record ApproveJoinRequestCommand(Guid RequestId) : ICommand<ResponseDto>;

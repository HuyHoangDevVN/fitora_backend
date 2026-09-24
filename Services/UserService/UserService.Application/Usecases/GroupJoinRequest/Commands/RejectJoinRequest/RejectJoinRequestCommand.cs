using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupJoinRequest.Commands.RejectJoinRequest;

public record RejectJoinRequestCommand(Guid RequestId, string? Reason) : ICommand<ResponseDto>;

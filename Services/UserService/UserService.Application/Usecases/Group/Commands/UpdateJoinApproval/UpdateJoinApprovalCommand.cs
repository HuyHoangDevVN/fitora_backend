using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.UpdateJoinApproval;

public record UpdateJoinApprovalCommand(Guid GroupId, bool RequireJoinApproval) : ICommand<ResponseDto>;

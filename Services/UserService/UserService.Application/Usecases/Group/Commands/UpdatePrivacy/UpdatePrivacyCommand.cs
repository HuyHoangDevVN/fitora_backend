using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.UpdatePrivacy;

public record UpdatePrivacyCommand(Guid GroupId, GroupPrivacy Privacy) : ICommand<ResponseDto>;

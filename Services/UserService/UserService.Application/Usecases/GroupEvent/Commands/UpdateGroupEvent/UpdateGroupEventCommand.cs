using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupEvent.Commands.UpdateGroupEvent;

public record UpdateGroupEventCommand(
    Guid EventId,
    string Title,
    string Description,
    DateTime EventDate,
    string? Location) : ICommand<ResponseDto>;

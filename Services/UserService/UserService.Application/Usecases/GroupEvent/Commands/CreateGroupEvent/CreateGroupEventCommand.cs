using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupEvent.Commands.CreateGroupEvent;

public record CreateGroupEventCommand(Guid GroupId, string Title, string Description, DateTime EventDate, string? Location) : ICommand<ResponseDto>;

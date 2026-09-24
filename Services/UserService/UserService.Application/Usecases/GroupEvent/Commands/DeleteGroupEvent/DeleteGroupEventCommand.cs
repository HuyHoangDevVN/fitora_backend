using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupEvent.Commands.DeleteGroupEvent;

public record DeleteGroupEventCommand(Guid EventId) : ICommand<ResponseDto>;

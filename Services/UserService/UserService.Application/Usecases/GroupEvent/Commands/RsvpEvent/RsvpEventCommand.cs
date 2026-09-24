using BuildingBlocks.DTOs;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupEvent.Commands.RsvpEvent;

public record RsvpEventCommand(Guid EventId, RsvpStatus Status) : ICommand<ResponseDto>;

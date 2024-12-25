
namespace AuthService.Domain.Events;

public record KeyCreateEvent(Key Key) : IDomainEvent;

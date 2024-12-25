

namespace AuthService.Domain.Events;

public record KeyUpdateEvent(Key Key) : IDomainEvent;

﻿using BuildingBlocks.Abstractions;

namespace AuthService.Domain.Abstractions;

public class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public IDomainEvent[] ClearDomainEvents()
    {
        IDomainEvent[] dqueuedEvents = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dqueuedEvents;
    }
}

using TodoApp.Domain.Events;

namespace TodoApp.Domain.Common;

/// <summary>
/// Base class for anything with an identity (an "entity" in the DDD sense:
/// two instances are equal if their Id matches, even if every other field differs).
/// Also lets any entity raise domain events without depending on a mediator/bus —
/// something in the Application/Infrastructure layer decides what to do with them later.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Id { get; set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

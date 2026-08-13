namespace TodoApp.Domain.Events;

/// <summary>
/// Marker interface for something that happened in the domain that other
/// parts of the system might care about (e.g. "a TodoItem was completed").
///
/// Deliberately NOT MediatR's INotification here — the Domain layer shouldn't
/// know MediatR exists. Later, in Application/Infrastructure, we'll adapt
/// these into whatever dispatch mechanism we choose. That's the whole trick
/// of the dependency rule: Domain defines the concept, outer layers wire it up.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

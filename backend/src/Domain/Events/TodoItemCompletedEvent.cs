using TodoApp.Domain.Entities;

namespace TodoApp.Domain.Events;

/// <summary>
/// Raised when a TodoItem transitions from not-done to done.
/// Nobody outside the Domain decides this happened — the entity itself
/// raises it, at the exact moment its own invariant (IsDone/CompletedAt)
/// changes. Infrastructure will later pick this up (e.g. via EF Core's
/// SaveChanges) and dispatch it — maybe to send a notification, maybe to
/// update a "completed today" counter. Domain doesn't know or care who's
/// listening; it just reports the fact.
/// </summary>
public class TodoItemCompletedEvent : IDomainEvent
{
    public TodoItemCompletedEvent(TodoItem item)
    {
        Item = item;
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public TodoItem Item { get; }

    public DateTimeOffset OccurredOn { get; }
}

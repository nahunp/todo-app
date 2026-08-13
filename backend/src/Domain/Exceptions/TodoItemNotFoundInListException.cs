namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Thrown when RenameItem/RemoveItem is called with a TodoItem that isn't
/// actually part of this list — e.g. an item that belongs to a different
/// TodoList, or one that was never added at all. Guards the aggregate
/// boundary: a TodoList should only ever mutate items it actually owns.
/// </summary>
public class TodoItemNotFoundInListException : DomainException
{
    public TodoItemNotFoundInListException()
        : base("This todo item does not belong to this list.")
    {
    }
}

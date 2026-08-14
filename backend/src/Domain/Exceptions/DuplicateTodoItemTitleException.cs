namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Thrown when adding or renaming a TodoItem would give it the same title
/// (case-insensitive, after trimming) as another item already in the same
/// TodoList. This is the whole reason TodoList exists as an aggregate root:
/// a standalone TodoItem has no visibility into its siblings, so this rule
/// can only be enforced one level up, where all the siblings are visible.
/// </summary>
public class DuplicateTodoItemTitleException : DomainException
{
    public DuplicateTodoItemTitleException(string title)
        : base($"A todo item titled '{title}' already exists in this list.")
    {
    }
}

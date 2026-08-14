namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Thrown when someone tries to give a TodoList an empty name or a name
/// that's too long. Same shape as TodoItemTitleInvalidException — raised
/// from inside the entity itself (see TodoList.SetName), never from a
/// controller or a validation attribute.
/// </summary>
public class TodoListNameInvalidException : DomainException
{
    public TodoListNameInvalidException(string message) : base(message)
    {
    }
}

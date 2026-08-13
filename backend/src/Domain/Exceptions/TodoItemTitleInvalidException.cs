namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Thrown when someone tries to give a TodoItem an empty title or a title
/// that's too long. Raised from inside the entity itself (see TodoItem.SetTitle),
/// never from a controller or a validation attribute — so it's enforced no
/// matter how the entity gets touched, today or in five years.
/// </summary>
public class TodoItemTitleInvalidException : DomainException
{
    public TodoItemTitleInvalidException(string message) : base(message)
    {
    }
}

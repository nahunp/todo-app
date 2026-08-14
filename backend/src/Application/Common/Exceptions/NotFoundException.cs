namespace TodoApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when a command/query targets an entity that doesn't exist —
/// e.g. a TodoListId that isn't in the database. Distinct from
/// TodoApp.Domain.Exceptions.DomainException (a business rule was
/// violated) and ValidationException (the request itself was malformed) —
/// this one means "the request was well-formed and valid, but what it
/// points at isn't there." GlobalExceptionHandler maps it to 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

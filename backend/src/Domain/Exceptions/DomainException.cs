namespace TodoApp.Domain.Exceptions;

/// <summary>
/// Base type for exceptions that mean "a business rule was violated" —
/// as opposed to a null reference, a timeout, a DB error, etc.
///
/// The payoff shows up later: the API layer can catch DomainException
/// specifically and turn it into a 400 Bad Request, while anything else
/// falls through as a 500. The exception type itself communicates intent.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

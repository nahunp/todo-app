using FluentValidation.Results;

namespace TodoApp.Application.Common.Exceptions;

/// <summary>
/// Thrown by ValidationBehaviour when a command/query fails FluentValidation
/// rules, before the handler ever runs. Deliberately a distinct type from
/// FluentValidation.ValidationException — this one groups failures by
/// property into the same shape ASP.NET's ValidationProblemDetails uses, so
/// the (future) API layer can catch it and return a clean 400 without any
/// translation step.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}

using FluentValidation;
using MediatR;
using ValidationException = TodoApp.Application.Common.Exceptions.ValidationException;

namespace TodoApp.Application.Common.Behaviours;

/// <summary>
/// A MediatR pipeline behaviour: runs for every request, before its handler.
/// This is what actually wires FluentValidation into the request pipeline —
/// without this, a *CommandValidator class just sits there unused, since
/// nothing calls .ValidateAsync() on its own. Registered once in
/// DependencyInjection.cs and it applies to every command/query from then on,
/// so individual handlers never validate anything themselves.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}

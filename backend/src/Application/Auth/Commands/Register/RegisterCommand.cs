using MediatR;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<string>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, errors) = await _identityService.CreateUserAsync(request.Email, request.Password);

        if (!succeeded)
        {
            // Reuses ValidationException's shape (property -> messages) even
            // though this isn't FluentValidation - Identity's own rejection
            // reasons (weak password, duplicate email) are exactly the same
            // kind of "well-formed request, still rejected" as a validator
            // failure, and the API layer already knows how to render this
            // shape as a 400.
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Registration"] = errors.ToArray(),
            });
        }

        return userId!;
    }
}

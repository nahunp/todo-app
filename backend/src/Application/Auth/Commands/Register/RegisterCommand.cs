using MediatR;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string CaptchaToken) : IRequest<string>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IIdentityService _identityService;
    private readonly ICaptchaService _captchaService;

    public RegisterCommandHandler(IIdentityService identityService, ICaptchaService captchaService)
    {
        _identityService = identityService;
        _captchaService = captchaService;
    }

    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Checked first, before ever touching Identity/the database — a bot
        // hammering this endpoint shouldn't cost a real CreateUserAsync
        // call (and its own DB round-trip) for every attempt.
        if (!await _captchaService.VerifyAsync(request.CaptchaToken, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["CaptchaToken"] = new[] { "Captcha verification failed. Please try again." },
            });
        }

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

using FluentValidation;

namespace TodoApp.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress();

        // Deliberately light here — Identity's own password policy (set on
        // AddIdentityCore in Infrastructure) is the real gate and returns
        // specific reasons (too short, needs a digit, etc.) via
        // IIdentityService.CreateUserAsync's Errors. This just catches the
        // obviously-empty case before bothering to call Identity at all.
        RuleFor(v => v.Password).NotEmpty();

        // Structural check only (is a token present at all) — whether it's
        // actually *valid* is an async call to Cloudflare, which happens in
        // the handler, not here. Same reasoning as Identity's password
        // policy above: this validator catches the free "obviously wrong"
        // case, the real gate lives past it.
        RuleFor(v => v.CaptchaToken).NotEmpty();
    }
}

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
        // AddIdentity in Infrastructure) is the real gate and returns
        // specific reasons (too short, needs a digit, etc.) via
        // IIdentityService.CreateUserAsync's Errors. This just catches the
        // obviously-empty case before bothering to call Identity at all.
        RuleFor(v => v.Password).NotEmpty();
    }
}

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TodoApp.Application.Auth.Commands.Login;
using TodoApp.Application.Auth.Commands.Register;

namespace TodoApp.WebApi.Auth;

// The frontend's live password-requirements checklist reads this instead
// of hardcoding a guess at Identity's policy — see DependencyInjection.cs's
// AddIdentityCore call for where these values actually come from.
public record PasswordPolicyResponse(
    int RequiredLength,
    bool RequireDigit,
    bool RequireLowercase,
    bool RequireUppercase,
    bool RequireNonAlphanumeric,
    int RequiredUniqueChars);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // No .RequireAuthorization() anywhere in this group, deliberately —
        // these are the only endpoints in the whole API you can call
        // without a token (register/login because they're how you get one;
        // password-policy because you need it before you have one).
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/auth/users/{userId}", new { id = userId });
        })
        .WithName("Register")
        .WithSummary("Creates a new user account. Requires a verified Turnstile CaptchaToken.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/password-policy", (IOptions<IdentityOptions> identityOptions) =>
        {
            var password = identityOptions.Value.Password;
            return Results.Ok(new PasswordPolicyResponse(
                password.RequiredLength,
                password.RequireDigit,
                password.RequireLowercase,
                password.RequireUppercase,
                password.RequireNonAlphanumeric,
                password.RequiredUniqueChars));
        })
        .WithName("GetPasswordPolicy")
        .WithSummary("Returns the active password policy, for building a live requirements checklist.")
        .Produces<PasswordPolicyResponse>();

        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Exchanges email/password for a JWT access token.")
        .Produces<LoginResult>()
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesValidationProblem();
    }
}

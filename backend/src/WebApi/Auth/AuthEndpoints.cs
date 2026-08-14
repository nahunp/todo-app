using MediatR;
using TodoApp.Application.Auth.Commands.Login;
using TodoApp.Application.Auth.Commands.Register;

namespace TodoApp.WebApi.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // No .RequireAuthorization() anywhere in this group, deliberately —
        // these are the only two endpoints in the whole API you can call
        // without a token, because they're how you get one.
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var userId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/auth/users/{userId}", new { id = userId });
        })
        .WithName("Register")
        .WithSummary("Creates a new user account.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem();

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

using MediatR;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.Auth.Commands.DeleteAccount;

// No parameters — deletes the *current* authenticated user, resolved from
// ICurrentUserService, same as every other owner-scoped operation in this
// app (see OwnershipExtensions.cs). There's no "delete someone else's
// account" concept here; if there ever needs to be an admin-deletes-a-user
// flow, that's a different command with different authorization, not this
// one.
public record DeleteAccountCommand : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public DeleteAccountCommandHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        // _currentUser.UserId is only ever null for contexts that bypass
        // .RequireAuthorization() entirely (see ICurrentUserService's own
        // doc comment) — this endpoint requires it, so in practice this
        // never actually throws in production; it's here so the compiler
        // (and anyone reading this later) doesn't have to trust that.
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No authenticated user.");

        // TodoList.OwnerId's foreign key cascades (see
        // TodoListConfiguration.cs) — deleting the Identity user deletes
        // their lists, and transitively their items, at the database
        // level. Nothing here has to walk and delete those manually.
        var succeeded = await _identityService.DeleteAccountAsync(userId);

        if (!succeeded)
        {
            // Shouldn't happen for a userId sourced from a just-validated
            // JWT, but IdentityResult failures (a concurrent delete, a
            // store-level error) are still real possibilities worth a
            // real error instead of silently no-op'ing. Deliberately not
            // DomainException (abstract, and means something more specific
            // — "a business rule was violated" — than "Identity's store
            // failed unexpectedly"). Falls through to
            // GlobalExceptionHandler's generic 500 case, which is the
            // correct outcome here: this isn't the caller's mistake.
            throw new InvalidOperationException("Could not delete the account.");
        }
    }
}

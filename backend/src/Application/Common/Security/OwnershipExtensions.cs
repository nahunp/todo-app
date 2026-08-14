using TodoApp.Application.Common.Exceptions;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Common.Security;

public static class OwnershipExtensions
{
    /// <summary>
    /// Throws NotFoundException — not a 403 — if the list isn't owned by
    /// the given user. Deliberately indistinguishable from "doesn't exist
    /// at all": a caller shouldn't be able to tell "this id belongs to
    /// someone else" apart from "this id was never real" (OWASP: don't
    /// leak a resource's existence to non-owners). Call this immediately
    /// after loading a TodoList in every command/query that takes one -
    /// it's the one check that's actually a security bug to forget, so it
    /// lives in one place instead of being retyped in eight handlers.
    /// </summary>
    public static void EnsureOwnedBy(this TodoList list, string? currentUserId)
    {
        if (list.OwnerId != currentUserId)
            throw new NotFoundException(nameof(TodoList), list.Id);
    }
}

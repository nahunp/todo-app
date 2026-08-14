namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Everything Application needs from user account management, expressed as
/// an interface so it never references ASP.NET Core Identity's types
/// directly (UserManager&lt;TUser&gt;, ApplicationUser) - those are
/// Infrastructure concerns, same reasoning as IApplicationDbContext keeping
/// Application off EF Core's SQL Server provider. Infrastructure implements
/// this against real Identity.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new user account. Errors is populated (and Succeeded false)
    /// for anything Identity itself rejects - duplicate email, password
    /// policy violations, etc. - so the caller can surface real reasons
    /// instead of a generic failure.
    /// </summary>
    Task<(bool Succeeded, string? UserId, IReadOnlyList<string> Errors)> CreateUserAsync(string email, string password);

    /// <summary>
    /// Null if the email/password combination is invalid - deliberately not
    /// distinguishing "no such user" from "wrong password" in the return
    /// value, same reasoning as TodoList ownership checks: don't leak which
    /// half of the pair was wrong.
    /// </summary>
    Task<string?> ValidateCredentialsAsync(string email, string password);
}

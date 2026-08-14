using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.UnitTests.Common;

/// <summary>
/// An in-memory stand-in for real Identity — same reasoning as
/// ApplicationDbContextFake using EF Core's InMemory provider instead of a
/// mock: this exercises real create/validate logic (duplicate email
/// rejection, password matching) rather than a hand-wired sequence of
/// .Returns() calls that could drift from what real Identity actually does.
/// </summary>
public class FakeIdentityService : IIdentityService
{
    private readonly Dictionary<string, (string UserId, string Password)> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);
    private int _nextId = 1;

    public Task<(bool Succeeded, string? UserId, IReadOnlyList<string> Errors)> CreateUserAsync(string email, string password)
    {
        if (_usersByEmail.ContainsKey(email))
            return Task.FromResult<(bool, string?, IReadOnlyList<string>)>((false, null, new[] { "Email is already registered." }));

        var userId = (_nextId++).ToString();
        _usersByEmail[email] = (userId, password);

        return Task.FromResult<(bool, string?, IReadOnlyList<string>)>((true, userId, Array.Empty<string>()));
    }

    public Task<string?> ValidateCredentialsAsync(string email, string password)
    {
        if (_usersByEmail.TryGetValue(email, out var user) && user.Password == password)
            return Task.FromResult<string?>(user.UserId);

        return Task.FromResult<string?>(null);
    }
}

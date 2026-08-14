using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.UnitTests.Common;

/// <summary>
/// A fixed, non-null user id for every test by default — tests that
/// specifically need to exercise "wrong user" pass a different id directly
/// where the handler under test needs one (see OwnershipTests-style cases).
/// </summary>
public class FakeCurrentUserService : ICurrentUserService
{
    public const string DefaultUserId = "test-user-1";

    public FakeCurrentUserService(string? userId = DefaultUserId)
    {
        UserId = userId;
    }

    public string? UserId { get; }
}

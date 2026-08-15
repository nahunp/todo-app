using TodoApp.Application.Auth.Commands.DeleteAccount;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.Auth.Commands.DeleteAccount;

public class DeleteAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesTheCurrentUsersAccount()
    {
        var identityService = new FakeIdentityService();
        var (_, userId, _) = await identityService.CreateUserAsync("user@example.com", "P@ssw0rd!");
        var handler = new DeleteAccountCommandHandler(identityService, new FakeCurrentUserService(userId));

        await handler.Handle(new DeleteAccountCommand(), CancellationToken.None);

        // The real proof it's gone, not just that DeleteAccountAsync
        // returned true — same reasoning as the live curl/sqlcmd
        // verification this feature got before it ever reached this test:
        // check the actual resulting state, not just that a call
        // "succeeded."
        var stillValid = await identityService.ValidateCredentialsAsync("user@example.com", "P@ssw0rd!");
        Assert.Null(stillValid);
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        var identityService = new FakeIdentityService();
        var handler = new DeleteAccountCommandHandler(identityService, new FakeCurrentUserService(userId: null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new DeleteAccountCommand(), CancellationToken.None));
    }
}

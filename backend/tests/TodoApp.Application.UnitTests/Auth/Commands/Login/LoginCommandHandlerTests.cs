using TodoApp.Application.Auth.Commands.Login;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private static async Task<(FakeIdentityService IdentityService, FakeTokenService TokenService, LoginCommandHandler Handler)> CreateHandlerWithRegisteredUserAsync(
        string email = "user@example.com", string password = "P@ssw0rd!")
    {
        var identityService = new FakeIdentityService();
        var tokenService = new FakeTokenService();
        await identityService.CreateUserAsync(email, password);
        return (identityService, tokenService, new LoginCommandHandler(identityService, tokenService));
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAccessToken()
    {
        var (_, _, handler) = await CreateHandlerWithRegisteredUserAsync();

        var result = await handler.Handle(new LoginCommand("user@example.com", "P@ssw0rd!"), CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        var (_, _, handler) = await CreateHandlerWithRegisteredUserAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("user@example.com", "WrongPassword"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsUnauthorizedAccessException()
    {
        var (_, _, handler) = await CreateHandlerWithRegisteredUserAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("nobody@example.com", "P@ssw0rd!"), CancellationToken.None));
    }
}

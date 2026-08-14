using TodoApp.Application.Auth.Commands.Register;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserAndReturnsId()
    {
        var handler = new RegisterCommandHandler(new FakeIdentityService());

        var userId = await handler.Handle(new RegisterCommand("new@example.com", "P@ssw0rd!"), CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(userId));
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ThrowsValidationException()
    {
        var identityService = new FakeIdentityService();
        var handler = new RegisterCommandHandler(identityService);
        await handler.Handle(new RegisterCommand("taken@example.com", "P@ssw0rd!"), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RegisterCommand("taken@example.com", "AnotherPass1!"), CancellationToken.None));

        Assert.Contains("Registration", ex.Errors.Keys);
    }
}

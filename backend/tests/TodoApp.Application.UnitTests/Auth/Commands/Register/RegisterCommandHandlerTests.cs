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
        var handler = new RegisterCommandHandler(new FakeIdentityService(), new FakeCaptchaService());

        var userId = await handler.Handle(new RegisterCommand("new@example.com", "P@ssw0rd!", "fake-token"), CancellationToken.None);

        Assert.False(string.IsNullOrEmpty(userId));
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ThrowsValidationException()
    {
        var identityService = new FakeIdentityService();
        var handler = new RegisterCommandHandler(identityService, new FakeCaptchaService());
        await handler.Handle(new RegisterCommand("taken@example.com", "P@ssw0rd!", "fake-token"), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RegisterCommand("taken@example.com", "AnotherPass1!", "fake-token"), CancellationToken.None));

        Assert.Contains("Registration", ex.Errors.Keys);
    }

    [Fact]
    public async Task Handle_WithFailedCaptcha_ThrowsValidationExceptionAndNeverCallsIdentity()
    {
        var identityService = new FakeIdentityService();
        var handler = new RegisterCommandHandler(identityService, new FakeCaptchaService(alwaysPasses: false));

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RegisterCommand("new@example.com", "P@ssw0rd!", "bad-token"), CancellationToken.None));

        Assert.Contains("CaptchaToken", ex.Errors.Keys);
        // Confirms the captcha check really does happen before Identity is
        // ever touched, not just that both eventually fail - a user with
        // this "taken" email is still available afterward, meaning
        // CreateUserAsync never ran.
        var stillAvailable = await identityService.CreateUserAsync("new@example.com", "P@ssw0rd!");
        Assert.True(stillAvailable.Succeeded);
    }
}

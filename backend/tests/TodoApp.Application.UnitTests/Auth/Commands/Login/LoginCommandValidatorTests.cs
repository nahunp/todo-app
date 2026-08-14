using FluentValidation.TestHelper;
using TodoApp.Application.Auth.Commands.Login;
using Xunit;

namespace TodoApp.Application.UnitTests.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginCommand("user@example.com", "P@ssw0rd!"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_HasError()
    {
        var result = _validator.TestValidate(new LoginCommand("", "P@ssw0rd!"));

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }
}

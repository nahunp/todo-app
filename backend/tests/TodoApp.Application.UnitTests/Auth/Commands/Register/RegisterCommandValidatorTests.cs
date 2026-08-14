using FluentValidation.TestHelper;
using TodoApp.Application.Auth.Commands.Register;
using Xunit;

namespace TodoApp.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new RegisterCommand("user@example.com", "P@ssw0rd!"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasError()
    {
        var result = _validator.TestValidate(new RegisterCommand("not-an-email", "P@ssw0rd!"));

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_HasError()
    {
        var result = _validator.TestValidate(new RegisterCommand("user@example.com", ""));

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }
}

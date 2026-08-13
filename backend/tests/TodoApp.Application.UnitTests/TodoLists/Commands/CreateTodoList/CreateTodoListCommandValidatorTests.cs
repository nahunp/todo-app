using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommandValidatorTests
{
    private readonly CreateTodoListCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidName_HasNoErrors()
    {
        var result = _validator.TestValidate(new CreateTodoListCommand("Groceries"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullName_HasError(string? name)
    {
        var result = _validator.TestValidate(new CreateTodoListCommand(name!));

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_WithNameOverMaxLength_HasError()
    {
        var tooLong = new string('a', 101);

        var result = _validator.TestValidate(new CreateTodoListCommand(tooLong));

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}

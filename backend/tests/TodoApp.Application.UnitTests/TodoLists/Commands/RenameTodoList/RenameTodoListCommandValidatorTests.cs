using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.RenameTodoList;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.RenameTodoList;

public class RenameTodoListCommandValidatorTests
{
    private readonly RenameTodoListCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new RenameTodoListCommand(1, "Weekend Groceries"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyNewName_HasError()
    {
        var result = _validator.TestValidate(new RenameTodoListCommand(1, ""));

        result.ShouldHaveValidationErrorFor(c => c.NewName);
    }
}

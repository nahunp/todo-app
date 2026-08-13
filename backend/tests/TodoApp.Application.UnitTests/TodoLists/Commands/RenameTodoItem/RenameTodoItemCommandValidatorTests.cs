using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.RenameTodoItem;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.RenameTodoItem;

public class RenameTodoItemCommandValidatorTests
{
    private readonly RenameTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new RenameTodoItemCommand(1, 1, "Buy oat milk"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyNewTitle_HasError()
    {
        var result = _validator.TestValidate(new RenameTodoItemCommand(1, 1, ""));

        result.ShouldHaveValidationErrorFor(c => c.NewTitle);
    }
}

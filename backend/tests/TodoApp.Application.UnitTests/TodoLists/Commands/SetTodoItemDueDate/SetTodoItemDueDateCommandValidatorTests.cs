using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.SetTodoItemDueDate;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.SetTodoItemDueDate;

public class SetTodoItemDueDateCommandValidatorTests
{
    private readonly SetTodoItemDueDateCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new SetTodoItemDueDateCommand(1, 1, DateTimeOffset.UtcNow));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullDueDate_HasNoErrors()
    {
        var result = _validator.TestValidate(new SetTodoItemDueDateCommand(1, 1, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithZeroTodoItemId_HasError()
    {
        var result = _validator.TestValidate(new SetTodoItemDueDateCommand(1, 0, null));

        result.ShouldHaveValidationErrorFor(c => c.TodoItemId);
    }
}

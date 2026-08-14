using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.AddTodoItem;

public class AddTodoItemCommandValidatorTests
{
    private readonly AddTodoItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new AddTodoItemCommand(1, "Buy milk"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_HasError()
    {
        var result = _validator.TestValidate(new AddTodoItemCommand(1, ""));

        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public void Validate_WithNonPositiveListId_HasError()
    {
        var result = _validator.TestValidate(new AddTodoItemCommand(0, "Buy milk"));

        result.ShouldHaveValidationErrorFor(c => c.TodoListId);
    }
}

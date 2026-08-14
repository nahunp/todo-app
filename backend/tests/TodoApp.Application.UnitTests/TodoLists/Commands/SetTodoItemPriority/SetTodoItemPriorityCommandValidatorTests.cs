using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.SetTodoItemPriority;
using TodoApp.Domain.Enums;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.SetTodoItemPriority;

public class SetTodoItemPriorityCommandValidatorTests
{
    private readonly SetTodoItemPriorityCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new SetTodoItemPriorityCommand(1, 1, PriorityLevel.High));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUndefinedPriority_HasError()
    {
        var result = _validator.TestValidate(new SetTodoItemPriorityCommand(1, 1, (PriorityLevel)99));

        result.ShouldHaveValidationErrorFor(c => c.Priority);
    }
}

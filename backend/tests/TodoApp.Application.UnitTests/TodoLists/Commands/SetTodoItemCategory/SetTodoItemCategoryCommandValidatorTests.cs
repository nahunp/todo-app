using FluentValidation.TestHelper;
using TodoApp.Application.TodoLists.Commands.SetTodoItemCategory;
using TodoApp.Domain.Enums;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.SetTodoItemCategory;

public class SetTodoItemCategoryCommandValidatorTests
{
    private readonly SetTodoItemCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new SetTodoItemCategoryCommand(1, 1, TodoItemCategory.Work));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUndefinedCategory_HasError()
    {
        var result = _validator.TestValidate(new SetTodoItemCategoryCommand(1, 1, (TodoItemCategory)99));

        result.ShouldHaveValidationErrorFor(c => c.Category);
    }
}

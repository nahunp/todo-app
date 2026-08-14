using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.SetTodoItemCategory;

public class SetTodoItemCategoryCommandValidator : AbstractValidator<SetTodoItemCategoryCommand>
{
    public SetTodoItemCategoryCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);
        RuleFor(v => v.Category).IsInEnum();
    }
}

using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.ReopenTodoItem;

public class ReopenTodoItemCommandValidator : AbstractValidator<ReopenTodoItemCommand>
{
    public ReopenTodoItemCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);
    }
}

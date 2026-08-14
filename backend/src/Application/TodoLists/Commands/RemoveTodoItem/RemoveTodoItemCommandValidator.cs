using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.RemoveTodoItem;

public class RemoveTodoItemCommandValidator : AbstractValidator<RemoveTodoItemCommand>
{
    public RemoveTodoItemCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);
    }
}

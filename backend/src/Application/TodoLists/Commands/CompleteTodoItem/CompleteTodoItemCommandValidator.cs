using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.CompleteTodoItem;

public class CompleteTodoItemCommandValidator : AbstractValidator<CompleteTodoItemCommand>
{
    public CompleteTodoItemCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);
    }
}

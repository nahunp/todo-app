using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.RenameTodoItem;

public class RenameTodoItemCommandValidator : AbstractValidator<RenameTodoItemCommand>
{
    public RenameTodoItemCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);

        RuleFor(v => v.NewTitle)
            .NotEmpty()
            .MaximumLength(200);
    }
}

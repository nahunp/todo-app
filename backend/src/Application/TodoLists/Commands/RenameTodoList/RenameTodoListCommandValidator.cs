using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.RenameTodoList;

public class RenameTodoListCommandValidator : AbstractValidator<RenameTodoListCommand>
{
    public RenameTodoListCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);

        RuleFor(v => v.NewName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

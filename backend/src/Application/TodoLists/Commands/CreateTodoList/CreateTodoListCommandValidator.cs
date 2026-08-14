using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
{
    public CreateTodoListCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

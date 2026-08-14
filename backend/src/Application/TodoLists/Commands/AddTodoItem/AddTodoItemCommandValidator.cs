using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.AddTodoItem;

public class AddTodoItemCommandValidator : AbstractValidator<AddTodoItemCommand>
{
    public AddTodoItemCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);

        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}

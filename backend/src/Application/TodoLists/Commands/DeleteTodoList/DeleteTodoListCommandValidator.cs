using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.DeleteTodoList;

public class DeleteTodoListCommandValidator : AbstractValidator<DeleteTodoListCommand>
{
    public DeleteTodoListCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
    }
}

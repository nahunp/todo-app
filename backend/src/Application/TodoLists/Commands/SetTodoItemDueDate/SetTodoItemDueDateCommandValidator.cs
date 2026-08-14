using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.SetTodoItemDueDate;

public class SetTodoItemDueDateCommandValidator : AbstractValidator<SetTodoItemDueDateCommand>
{
    public SetTodoItemDueDateCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);

        // DueDate itself is intentionally unconstrained — null clears it,
        // and TodoItem.SetDueDate puts no limit on past/future values (a
        // "log this overdue thing" use case is legitimate, not an error).
    }
}

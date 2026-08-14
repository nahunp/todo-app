using FluentValidation;

namespace TodoApp.Application.TodoLists.Commands.SetTodoItemPriority;

public class SetTodoItemPriorityCommandValidator : AbstractValidator<SetTodoItemPriorityCommand>
{
    public SetTodoItemPriorityCommandValidator()
    {
        RuleFor(v => v.TodoListId).GreaterThan(0);
        RuleFor(v => v.TodoItemId).GreaterThan(0);

        // Binds from JSON as an int under the hood — a genuinely malformed
        // enum name fails deserialization (400) before this ever runs, but
        // an in-range-for-int, out-of-range-for-the-enum value like 99
        // would bind successfully without this check.
        RuleFor(v => v.Priority).IsInEnum();
    }
}

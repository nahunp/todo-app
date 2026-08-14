using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;

namespace TodoApp.Application.TodoLists.Commands.SetTodoItemPriority;

public record SetTodoItemPriorityCommand(int TodoListId, int TodoItemId, PriorityLevel Priority) : IRequest;

public class SetTodoItemPriorityCommandHandler : IRequestHandler<SetTodoItemPriorityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetTodoItemPriorityCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetTodoItemPriorityCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        // Called directly on the item, not routed through TodoList — same
        // reasoning as CompleteTodoItem/ReopenTodoItem: no cross-item
        // invariant for the aggregate root to enforce here.
        item.ChangePriority(request.Priority);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.CompleteTodoItem;

public record CompleteTodoItemCommand(int TodoListId, int TodoItemId) : IRequest;

public class CompleteTodoItemCommandHandler : IRequestHandler<CompleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CompleteTodoItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CompleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        // Called directly on the item, not routed through TodoList — unlike
        // Rename/duplicate-title, completion has no cross-item invariant for
        // the aggregate root to enforce. MarkComplete is already a no-op if
        // the item is already done (see TodoItem.cs).
        item.MarkComplete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

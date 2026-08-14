using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.RenameTodoItem;

public record RenameTodoItemCommand(int TodoListId, int TodoItemId, string NewTitle) : IRequest;

public class RenameTodoItemCommandHandler : IRequestHandler<RenameTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RenameTodoItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // IRequest (no <TResponse>) pairs with a Handle that returns plain
    // Task, not Task<Unit> — that mismatch is exactly what broke the
    // discarded first attempt at this (CS0738).
    public async Task Handle(RenameTodoItemCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        // TodoList.RenameItem, not item.Rename directly — the whole point
        // of the aggregate root is the cross-item uniqueness check that
        // only TodoList can do.
        list.RenameItem(item, request.NewTitle);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

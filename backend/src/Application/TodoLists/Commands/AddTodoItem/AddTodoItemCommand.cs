using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;

namespace TodoApp.Application.TodoLists.Commands.AddTodoItem;

public record AddTodoItemCommand(
    int TodoListId,
    string Title,
    string? Notes = null,
    PriorityLevel Priority = PriorityLevel.Medium,
    DateTimeOffset? DueDate = null,
    TodoItemCategory Category = TodoItemCategory.None) : IRequest<int>;

public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddTodoItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AddTodoItemCommand request, CancellationToken cancellationToken)
    {
        // Include(l => l.Items) works despite Items being a computed,
        // get-only property — EF Core resolves it against the Items
        // navigation registered in TodoListConfiguration (backed by the
        // private _items field), not against the property's own getter
        // body. No EF.Property<T> hack needed.
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        // AddItem enforces the list's own invariant (no duplicate titles)
        // and TodoItem's (title non-empty/length) — both DomainExceptions,
        // both already mapped to 400 by GlobalExceptionHandler.
        var item = list.AddItem(request.Title, request.Notes, request.Priority, request.DueDate, request.Category);

        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}

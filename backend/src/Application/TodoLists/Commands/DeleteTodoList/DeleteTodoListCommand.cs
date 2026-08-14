using MediatR;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.DeleteTodoList;

public record DeleteTodoListCommand(int TodoListId) : IRequest;

public class DeleteTodoListCommandHandler : IRequestHandler<DeleteTodoListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTodoListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        // No .Include(l => l.Items) needed — we're not touching Items
        // ourselves, just removing the list. The cascade delete for its
        // items is configured at the database level (TodoListConfiguration:
        // HasMany(...).WithOne().IsRequired().OnDelete(DeleteBehavior.Cascade)),
        // so SQL Server removes the child rows, not this handler.
        var list = await _context.TodoLists.FindAsync(new object[] { request.TodoListId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        _context.TodoLists.Remove(list);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.RenameTodoList;

public record RenameTodoListCommand(int TodoListId, string NewName) : IRequest;

public class RenameTodoListCommandHandler : IRequestHandler<RenameTodoListCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RenameTodoListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RenameTodoListCommand request, CancellationToken cancellationToken)
    {
        // No .Include(l => l.Items) here — renaming the list itself
        // doesn't touch Items, and EF Core would need to load an
        // (currently) unused navigation for nothing.
        var list = await _context.TodoLists
            .FindAsync(new object[] { request.TodoListId }, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        list.Rename(request.NewName);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

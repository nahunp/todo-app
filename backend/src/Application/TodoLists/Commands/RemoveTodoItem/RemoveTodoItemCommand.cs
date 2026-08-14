using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.RemoveTodoItem;

public record RemoveTodoItemCommand(int TodoListId, int TodoItemId) : IRequest;

public class RemoveTodoItemCommandHandler : IRequestHandler<RemoveTodoItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RemoveTodoItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveTodoItemCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        list.RemoveItem(item);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

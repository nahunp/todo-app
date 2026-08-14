using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.ReopenTodoItem;

public record ReopenTodoItemCommand(int TodoListId, int TodoItemId) : IRequest;

public class ReopenTodoItemCommandHandler : IRequestHandler<ReopenTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public ReopenTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReopenTodoItemCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        item.Reopen();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

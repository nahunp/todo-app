using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.RemoveTodoItem;

public record RemoveTodoItemCommand(int TodoListId, int TodoItemId) : IRequest;

public class RemoveTodoItemCommandHandler : IRequestHandler<RemoveTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveTodoItemCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        var item = list.Items.FirstOrDefault(i => i.Id == request.TodoItemId)
            ?? throw new NotFoundException(nameof(TodoItem), request.TodoItemId);

        list.RemoveItem(item);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

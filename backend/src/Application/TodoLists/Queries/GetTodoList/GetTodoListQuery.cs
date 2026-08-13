using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;

namespace TodoApp.Application.TodoLists.Queries.GetTodoList;

public record GetTodoListQuery(int TodoListId) : IRequest<TodoListDetailDto>;

public record TodoItemDto(
    int Id,
    string Title,
    string? Notes,
    bool IsDone,
    DateTimeOffset? CompletedAt,
    PriorityLevel Priority,
    DateTimeOffset? DueDate);

public record TodoListDetailDto(int Id, string Name, List<TodoItemDto> Items);

public class GetTodoListQueryHandler : IRequestHandler<GetTodoListQuery, TodoListDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetTodoListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodoListDetailDto> Handle(GetTodoListQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        return new TodoListDetailDto(
            list.Id,
            list.Name,
            list.Items
                .Select(i => new TodoItemDto(i.Id, i.Title, i.Notes, i.IsDone, i.CompletedAt, i.Priority, i.DueDate))
                .ToList());
    }
}

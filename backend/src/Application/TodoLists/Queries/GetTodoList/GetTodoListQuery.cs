using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Application.Common.Security;
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
    DateTimeOffset? DueDate,
    TodoItemCategory Category,
    // Computed at read time from DueDate, not a stored column — see
    // TodoItem.GetDueDateState. Included here so every client (this
    // frontend, and eventually Android/iOS) shows the same Overdue/Today/
    // Upcoming without each re-implementing the date math itself.
    DueDateState DueDateState);

public record TodoListDetailDto(int Id, string Name, List<TodoItemDto> Items);

public class GetTodoListQueryHandler : IRequestHandler<GetTodoListQuery, TodoListDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTodoListQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TodoListDetailDto> Handle(GetTodoListQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == request.TodoListId, cancellationToken)
            ?? throw new NotFoundException(nameof(TodoList), request.TodoListId);

        list.EnsureOwnedBy(_currentUser.UserId);

        // One "now" for the whole projection, not one UtcNow call per item —
        // so two items due exactly at today's boundary can't land on
        // opposite sides of it just because a few ticks elapsed evaluating
        // the LINQ between them.
        var now = DateTimeOffset.UtcNow;

        return new TodoListDetailDto(
            list.Id,
            list.Name,
            list.Items
                .Select(i => new TodoItemDto(i.Id, i.Title, i.Notes, i.IsDone, i.CompletedAt, i.Priority, i.DueDate, i.Category, i.GetDueDateState(now)))
                .ToList());
    }
}

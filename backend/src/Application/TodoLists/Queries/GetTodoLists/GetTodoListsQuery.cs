using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.Application.TodoLists.Queries.GetTodoLists;

public record GetTodoListsQuery : IRequest<List<TodoListDto>>;

/// <summary>
/// Shape the API actually returns — deliberately not TodoList itself.
/// Serializing domain entities directly over the wire couples the API
/// contract to internal structure (private setters aside, EF Core
/// navigation properties are exactly the kind of thing that causes
/// surprises in JSON output) and makes "what does the frontend see" someone
/// has to trace through the entity instead of read directly here.
/// </summary>
public record TodoListDto(int Id, string Name);

public class GetTodoListsQueryHandler : IRequestHandler<GetTodoListsQuery, List<TodoListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTodoListsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TodoListDto>> Handle(GetTodoListsQuery request, CancellationToken cancellationToken)
    {
        // Filtered here, not checked-per-item after loading everything —
        // "list all your lists" should never see another user's rows in
        // the first place, not filter them out after the fact.
        return await _context.TodoLists
            .Where(l => l.OwnerId == _currentUser.UserId)
            .OrderBy(l => l.Name)
            .Select(l => new TodoListDto(l.Id, l.Name))
            .ToListAsync(cancellationToken);
    }
}

using MediatR;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.TodoLists.Commands.CreateTodoList;

/// <summary>
/// A record, not a class — commands are just data, immutable once created.
/// IRequest&lt;int&gt; means "handling this returns an int" (the new list's Id).
/// </summary>
public record CreateTodoListCommand(string Name) : IRequest<int>;

public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTodoListCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        // By the time we get here, ValidationBehaviour has already confirmed
        // request.Name passes CreateTodoListCommandValidator's rules. The
        // TodoList constructor validates again (see SetName) — that's not
        // redundant, it's a different question being answered: the validator
        // guards "was this a well-formed request?" (so the API layer can
        // return a clean 400 with field-level errors); the entity's own
        // check guards "can a TodoList ever exist in an invalid state?",
        // no matter what code constructs one, today or in five years.
        //
        // _currentUser.UserId! - the endpoint is .RequireAuthorization()'d,
        // so a null UserId here would mean the auth pipeline itself is
        // broken, not something this handler should try to handle gracefully.
        var entity = new TodoList(_currentUser.UserId!, request.Name);

        _context.TodoLists.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

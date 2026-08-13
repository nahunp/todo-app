using MediatR;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Queries.GetTodoLists;

namespace TodoApp.WebApi.TodoLists;

public static class TodoListEndpoints
{
    public static void MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todolists").WithTags("TodoLists");

        // ISender, not IMediator — endpoints only ever Send commands/queries
        // here, never Publish notifications directly.
        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var lists = await sender.Send(new GetTodoListsQuery(), cancellationToken);
            return Results.Ok(lists);
        })
        .WithName("GetTodoLists")
        .WithSummary("Lists all todo lists.")
        .Produces<List<TodoListDto>>();

        group.MapPost("/", async (CreateTodoListCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/todolists/{id}", new { id });
        })
        .WithName("CreateTodoList")
        .WithSummary("Creates a new todo list.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}

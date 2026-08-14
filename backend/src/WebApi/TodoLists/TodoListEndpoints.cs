using MediatR;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CompleteTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.RemoveTodoItem;
using TodoApp.Application.TodoLists.Commands.RenameTodoItem;
using TodoApp.Application.TodoLists.Commands.RenameTodoList;
using TodoApp.Application.TodoLists.Commands.ReopenTodoItem;
using TodoApp.Application.TodoLists.Queries.GetTodoList;
using TodoApp.Application.TodoLists.Queries.GetTodoLists;
using TodoApp.Domain.Enums;

namespace TodoApp.WebApi.TodoLists;

// Request bodies for routes where the id comes from the URL, not the body —
// binding the command record directly would mean the client has to repeat
// the id in the body too, which invites the two disagreeing.
public record AddTodoItemRequest(string Title, string? Notes = null, PriorityLevel Priority = PriorityLevel.Medium, DateTimeOffset? DueDate = null);
public record RenameTodoItemRequest(string NewTitle);
public record RenameTodoListRequest(string NewName);

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

        group.MapGet("/{id:int}", async (int id, ISender sender, CancellationToken cancellationToken) =>
        {
            var list = await sender.Send(new GetTodoListQuery(id), cancellationToken);
            return Results.Ok(list);
        })
        .WithName("GetTodoList")
        .WithSummary("Gets a single todo list, including its items.")
        .Produces<TodoListDetailDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateTodoListCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/todolists/{id}", new { id });
        })
        .WithName("CreateTodoList")
        .WithSummary("Creates a new todo list.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapPatch("/{id:int}", async (int id, RenameTodoListRequest body, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new RenameTodoListCommand(id, body.NewName), cancellationToken);
            return Results.NoContent();
        })
        .WithName("RenameTodoList")
        .WithSummary("Renames a todo list.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/items", async (int id, AddTodoItemRequest body, ISender sender, CancellationToken cancellationToken) =>
        {
            var itemId = await sender.Send(new AddTodoItemCommand(id, body.Title, body.Notes, body.Priority, body.DueDate), cancellationToken);
            return Results.Created($"/api/todolists/{id}/items/{itemId}", new { id = itemId });
        })
        .WithName("AddTodoItem")
        .WithSummary("Adds a new item to a todo list.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/items/{itemId:int}", async (int id, int itemId, RenameTodoItemRequest body, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new RenameTodoItemCommand(id, itemId, body.NewTitle), cancellationToken);
            return Results.NoContent();
        })
        .WithName("RenameTodoItem")
        .WithSummary("Renames an item in a todo list.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}/items/{itemId:int}", async (int id, int itemId, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new RemoveTodoItemCommand(id, itemId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("RemoveTodoItem")
        .WithSummary("Removes an item from a todo list.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        // POST, not PATCH — this isn't "update the IsDone field," it's an
        // action/transition (see TodoItem.MarkComplete/Reopen: both raise a
        // domain event and enforce IsDone/CompletedAt move together). No
        // request body needed, everything comes from the route.
        group.MapPost("/{id:int}/items/{itemId:int}/complete", async (int id, int itemId, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new CompleteTodoItemCommand(id, itemId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("CompleteTodoItem")
        .WithSummary("Marks an item as done.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/items/{itemId:int}/reopen", async (int id, int itemId, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new ReopenTodoItemCommand(id, itemId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("ReopenTodoItem")
        .WithSummary("Reopens a completed item.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

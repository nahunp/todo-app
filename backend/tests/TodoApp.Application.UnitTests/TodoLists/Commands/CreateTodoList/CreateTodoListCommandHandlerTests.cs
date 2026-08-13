using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.CreateTodoList;

public class CreateTodoListCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesTodoListWithGivenName()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new CreateTodoListCommandHandler(context);

        var id = await handler.Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var created = context.TodoLists.Single(l => l.Id == id);
        Assert.Equal("Groceries", created.Name);
    }

    [Fact]
    public async Task Handle_ReturnsAPersistedId()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new CreateTodoListCommandHandler(context);

        var id = await handler.Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        // 0 is TodoList.Id's default, unsaved value — a real SaveChangesAsync
        // must have assigned a real one for this to be anything else.
        Assert.NotEqual(0, id);
    }

    [Fact]
    public async Task Handle_NewListStartsWithNoItems()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new CreateTodoListCommandHandler(context);

        var id = await handler.Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var created = context.TodoLists.Single(l => l.Id == id);
        Assert.Empty(created.Items);
    }

    [Fact]
    public async Task Handle_WithInvalidName_PropagatesTodoListNameInvalidException()
    {
        // ValidationBehaviour isn't in play here — the handler is invoked
        // directly, the way MediatR would only after validation passes. So
        // this exercises TodoList's own invariant (SetName), the same
        // defence-in-depth point noted in CreateTodoListCommandHandler.
        var context = ApplicationDbContextFake.Create();
        var handler = new CreateTodoListCommandHandler(context);

        await Assert.ThrowsAsync<TodoApp.Domain.Exceptions.TodoListNameInvalidException>(
            () => handler.Handle(new CreateTodoListCommand(""), CancellationToken.None));
    }
}

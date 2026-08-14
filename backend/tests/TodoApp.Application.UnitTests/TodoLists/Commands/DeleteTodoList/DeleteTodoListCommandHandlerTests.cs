using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.DeleteTodoList;
using TodoApp.Application.UnitTests.Common;
using TodoApp.Domain.Entities;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.DeleteTodoList;

public class DeleteTodoListCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesTheList()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new DeleteTodoListCommandHandler(context);

        await handler.Handle(new DeleteTodoListCommand(listId), CancellationToken.None);

        Assert.Null(context.TodoLists.SingleOrDefault(l => l.Id == listId));
    }

    [Fact]
    public async Task Handle_WithItems_DeletesTheListAndItsItems()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        await new AddTodoItemCommandHandler(context).Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);
        var handler = new DeleteTodoListCommandHandler(context);

        await handler.Handle(new DeleteTodoListCommand(listId), CancellationToken.None);

        // No dedicated DbSet<TodoItem> on IApplicationDbContext by design
        // (items are only ever reached through TodoList.Items) - Set<T>()
        // is EF Core's own escape hatch for exactly this, checking the
        // cascade actually happened at the database level.
        Assert.Empty(context.Set<TodoItem>());
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new DeleteTodoListCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteTodoListCommand(999), CancellationToken.None));
    }
}

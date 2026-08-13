using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.RemoveTodoItem;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.RemoveTodoItem;

public class RemoveTodoItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, int ListId, int ItemId)> CreateListWithItemAsync()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context).Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);
        return (context, listId, itemId);
    }

    [Fact]
    public async Task Handle_RemovesTheItem()
    {
        var (context, listId, itemId) = await CreateListWithItemAsync();
        var handler = new RemoveTodoItemCommandHandler(context);

        await handler.Handle(new RemoveTodoItemCommand(listId, itemId), CancellationToken.None);

        Assert.Empty(context.TodoLists.Single(l => l.Id == listId).Items);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new RemoveTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RemoveTodoItemCommand(999, 1), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var (context, listId, _) = await CreateListWithItemAsync();
        var handler = new RemoveTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RemoveTodoItemCommand(listId, 999), CancellationToken.None));
    }
}

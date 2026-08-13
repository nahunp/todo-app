using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.RenameTodoItem;
using TodoApp.Application.UnitTests.Common;
using TodoApp.Domain.Exceptions;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.RenameTodoItem;

public class RenameTodoItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, int ListId, int ItemId)> CreateListWithItemAsync(string itemTitle = "Buy milk")
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context).Handle(new AddTodoItemCommand(listId, itemTitle), CancellationToken.None);
        return (context, listId, itemId);
    }

    [Fact]
    public async Task Handle_RenamesTheItem()
    {
        var (context, listId, itemId) = await CreateListWithItemAsync();
        var handler = new RenameTodoItemCommandHandler(context);

        await handler.Handle(new RenameTodoItemCommand(listId, itemId, "Buy oat milk"), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.Equal("Buy oat milk", item.Title);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new RenameTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RenameTodoItemCommand(999, 1, "New title"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var (context, listId, _) = await CreateListWithItemAsync();
        var handler = new RenameTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RenameTodoItemCommand(listId, 999, "New title"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDuplicateTitle_PropagatesDuplicateTodoItemTitleException()
    {
        var (context, listId, itemId) = await CreateListWithItemAsync();
        await new AddTodoItemCommandHandler(context).Handle(new AddTodoItemCommand(listId, "Buy bread"), CancellationToken.None);
        var handler = new RenameTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<DuplicateTodoItemTitleException>(
            () => handler.Handle(new RenameTodoItemCommand(listId, itemId, "Buy bread"), CancellationToken.None));
    }
}

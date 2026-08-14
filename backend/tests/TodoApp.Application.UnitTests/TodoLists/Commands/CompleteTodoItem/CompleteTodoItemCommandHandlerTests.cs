using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CompleteTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.CompleteTodoItem;

public class CompleteTodoItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, int ListId, int ItemId)> CreateListWithItemAsync()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context).Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);
        return (context, listId, itemId);
    }

    [Fact]
    public async Task Handle_MarksTheItemDone()
    {
        var (context, listId, itemId) = await CreateListWithItemAsync();
        var handler = new CompleteTodoItemCommandHandler(context);

        await handler.Handle(new CompleteTodoItemCommand(listId, itemId), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.True(item.IsDone);
        Assert.NotNull(item.CompletedAt);
    }

    [Fact]
    public async Task Handle_WhenAlreadyDone_IsNoOp()
    {
        var (context, listId, itemId) = await CreateListWithItemAsync();
        var handler = new CompleteTodoItemCommandHandler(context);
        await handler.Handle(new CompleteTodoItemCommand(listId, itemId), CancellationToken.None);
        var firstCompletedAt = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId).CompletedAt;

        await handler.Handle(new CompleteTodoItemCommand(listId, itemId), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.Equal(firstCompletedAt, item.CompletedAt);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new CompleteTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new CompleteTodoItemCommand(999, 1), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var (context, listId, _) = await CreateListWithItemAsync();
        var handler = new CompleteTodoItemCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new CompleteTodoItemCommand(listId, 999), CancellationToken.None));
    }
}

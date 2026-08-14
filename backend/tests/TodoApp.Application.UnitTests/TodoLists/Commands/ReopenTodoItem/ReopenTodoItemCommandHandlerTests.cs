using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.ReopenTodoItem;
using TodoApp.Application.UnitTests.Common;
using Xunit;
using CompleteTodoItemCommand = TodoApp.Application.TodoLists.Commands.CompleteTodoItem.CompleteTodoItemCommand;
using CompleteTodoItemCommandHandler = TodoApp.Application.TodoLists.Commands.CompleteTodoItem.CompleteTodoItemCommandHandler;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.ReopenTodoItem;

public class ReopenTodoItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, FakeCurrentUserService CurrentUser, int ListId, int ItemId)> CreateCompletedItemAsync()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context, currentUser).Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);
        await new CompleteTodoItemCommandHandler(context, currentUser).Handle(new CompleteTodoItemCommand(listId, itemId), CancellationToken.None);
        return (context, currentUser, listId, itemId);
    }

    [Fact]
    public async Task Handle_ClearsIsDoneAndCompletedAt()
    {
        var (context, currentUser, listId, itemId) = await CreateCompletedItemAsync();
        var handler = new ReopenTodoItemCommandHandler(context, currentUser);

        await handler.Handle(new ReopenTodoItemCommand(listId, itemId), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.False(item.IsDone);
        Assert.Null(item.CompletedAt);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new ReopenTodoItemCommandHandler(context, new FakeCurrentUserService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new ReopenTodoItemCommand(999, 1), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new ReopenTodoItemCommandHandler(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new ReopenTodoItemCommand(listId, 999), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OnAnotherUsersList_ThrowsNotFoundException()
    {
        var (context, _, listId, itemId) = await CreateCompletedItemAsync();
        var handler = new ReopenTodoItemCommandHandler(context, new FakeCurrentUserService("someone-else"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new ReopenTodoItemCommand(listId, itemId), CancellationToken.None));
    }
}

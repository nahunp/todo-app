using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Queries.GetTodoList;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Queries.GetTodoList;

public class GetTodoListQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheListWithNoItems()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new GetTodoListQueryHandler(context, currentUser);

        var result = await handler.Handle(new GetTodoListQuery(listId), CancellationToken.None);

        Assert.Equal(listId, result.Id);
        Assert.Equal("Groceries", result.Name);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_ReturnsItemsMappedCorrectly()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context, currentUser).Handle(new AddTodoItemCommand(listId, "Buy milk", "2%"), CancellationToken.None);
        var handler = new GetTodoListQueryHandler(context, currentUser);

        var result = await handler.Handle(new GetTodoListQuery(listId), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(itemId, item.Id);
        Assert.Equal("Buy milk", item.Title);
        Assert.Equal("2%", item.Notes);
        Assert.False(item.IsDone);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new GetTodoListQueryHandler(context, new FakeCurrentUserService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTodoListQuery(999), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OnAnotherUsersList_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context, new FakeCurrentUserService()).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new GetTodoListQueryHandler(context, new FakeCurrentUserService("someone-else"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTodoListQuery(listId), CancellationToken.None));
    }
}

using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.SetTodoItemDueDate;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.SetTodoItemDueDate;

public class SetTodoItemDueDateCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, FakeCurrentUserService CurrentUser, int ListId, int ItemId)> CreateListWithItemAsync()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var itemId = await new AddTodoItemCommandHandler(context, currentUser).Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);
        return (context, currentUser, listId, itemId);
    }

    [Fact]
    public async Task Handle_SetsDueDate()
    {
        var (context, currentUser, listId, itemId) = await CreateListWithItemAsync();
        var handler = new SetTodoItemDueDateCommandHandler(context, currentUser);
        var dueDate = DateTimeOffset.UtcNow.AddDays(5);

        await handler.Handle(new SetTodoItemDueDateCommand(listId, itemId, dueDate), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.Equal(dueDate, item.DueDate);
    }

    [Fact]
    public async Task Handle_WithNull_ClearsExistingDueDate()
    {
        var (context, currentUser, listId, itemId) = await CreateListWithItemAsync();
        var handler = new SetTodoItemDueDateCommandHandler(context, currentUser);
        await handler.Handle(new SetTodoItemDueDateCommand(listId, itemId, DateTimeOffset.UtcNow), CancellationToken.None);

        await handler.Handle(new SetTodoItemDueDateCommand(listId, itemId, null), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.Null(item.DueDate);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new SetTodoItemDueDateCommandHandler(context, new FakeCurrentUserService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemDueDateCommand(999, 1, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var (context, currentUser, listId, _) = await CreateListWithItemAsync();
        var handler = new SetTodoItemDueDateCommandHandler(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemDueDateCommand(listId, 999, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OnAnotherUsersList_ThrowsNotFoundException()
    {
        var (context, _, listId, itemId) = await CreateListWithItemAsync();
        var handler = new SetTodoItemDueDateCommandHandler(context, new FakeCurrentUserService("someone-else"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemDueDateCommand(listId, itemId, null), CancellationToken.None));
    }
}

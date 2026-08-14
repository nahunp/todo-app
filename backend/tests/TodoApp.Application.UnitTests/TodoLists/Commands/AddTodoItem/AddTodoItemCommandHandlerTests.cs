using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.UnitTests.Common;
using TodoApp.Domain.Enums;
using TodoApp.Domain.Exceptions;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.AddTodoItem;

public class AddTodoItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContextFake Context, FakeCurrentUserService CurrentUser, int ListId)> CreateListAsync(string name = "Groceries")
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var listId = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand(name), CancellationToken.None);
        return (context, currentUser, listId);
    }

    [Fact]
    public async Task Handle_AddsItemToTheList()
    {
        var (context, currentUser, listId) = await CreateListAsync();
        var handler = new AddTodoItemCommandHandler(context, currentUser);

        var itemId = await handler.Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);

        var list = context.TodoLists.Single(l => l.Id == listId);
        var item = Assert.Single(list.Items);
        Assert.Equal(itemId, item.Id);
        Assert.Equal("Buy milk", item.Title);
    }

    [Fact]
    public async Task Handle_WithPriorityAndDueDate_SetsThem()
    {
        var (context, currentUser, listId) = await CreateListAsync();
        var handler = new AddTodoItemCommandHandler(context, currentUser);
        var dueDate = DateTimeOffset.UtcNow.AddDays(3);

        await handler.Handle(new AddTodoItemCommand(listId, "Buy milk", Priority: PriorityLevel.High, DueDate: dueDate), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single();
        Assert.Equal(PriorityLevel.High, item.Priority);
        Assert.Equal(dueDate, item.DueDate);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new AddTodoItemCommandHandler(context, new FakeCurrentUserService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new AddTodoItemCommand(999, "Buy milk"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDuplicateTitleInList_PropagatesDuplicateTodoItemTitleException()
    {
        var (context, currentUser, listId) = await CreateListAsync();
        var handler = new AddTodoItemCommandHandler(context, currentUser);
        await handler.Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None);

        await Assert.ThrowsAsync<DuplicateTodoItemTitleException>(
            () => handler.Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OnAnotherUsersList_ThrowsNotFoundException()
    {
        var (context, _, listId) = await CreateListAsync();
        var someoneElse = new FakeCurrentUserService("someone-else");
        var handler = new AddTodoItemCommandHandler(context, someoneElse);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new AddTodoItemCommand(listId, "Buy milk"), CancellationToken.None));
    }
}

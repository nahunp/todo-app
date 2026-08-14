using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.AddTodoItem;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.SetTodoItemCategory;
using TodoApp.Application.UnitTests.Common;
using TodoApp.Domain.Enums;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.SetTodoItemCategory;

public class SetTodoItemCategoryCommandHandlerTests
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
    public async Task Handle_UpdatesCategory()
    {
        var (context, currentUser, listId, itemId) = await CreateListWithItemAsync();
        var handler = new SetTodoItemCategoryCommandHandler(context, currentUser);

        await handler.Handle(new SetTodoItemCategoryCommand(listId, itemId, TodoItemCategory.Health), CancellationToken.None);

        var item = context.TodoLists.Single(l => l.Id == listId).Items.Single(i => i.Id == itemId);
        Assert.Equal(TodoItemCategory.Health, item.Category);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new SetTodoItemCategoryCommandHandler(context, new FakeCurrentUserService());

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemCategoryCommand(999, 1, TodoItemCategory.Work), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownItemId_ThrowsNotFoundException()
    {
        var (context, currentUser, listId, _) = await CreateListWithItemAsync();
        var handler = new SetTodoItemCategoryCommandHandler(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemCategoryCommand(listId, 999, TodoItemCategory.Work), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OnAnotherUsersList_ThrowsNotFoundException()
    {
        var (context, _, listId, itemId) = await CreateListWithItemAsync();
        var handler = new SetTodoItemCategoryCommandHandler(context, new FakeCurrentUserService("someone-else"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new SetTodoItemCategoryCommand(listId, itemId, TodoItemCategory.Work), CancellationToken.None));
    }
}

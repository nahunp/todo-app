using TodoApp.Application.Common.Exceptions;
using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Commands.RenameTodoList;
using TodoApp.Application.UnitTests.Common;
using TodoApp.Domain.Exceptions;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Commands.RenameTodoList;

public class RenameTodoListCommandHandlerTests
{
    [Fact]
    public async Task Handle_RenamesTheList()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new RenameTodoListCommandHandler(context);

        await handler.Handle(new RenameTodoListCommand(listId, "Weekend Groceries"), CancellationToken.None);

        Assert.Equal("Weekend Groceries", context.TodoLists.Single(l => l.Id == listId).Name);
    }

    [Fact]
    public async Task Handle_WithUnknownListId_ThrowsNotFoundException()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new RenameTodoListCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new RenameTodoListCommand(999, "New name"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInvalidName_PropagatesTodoListNameInvalidException()
    {
        var context = ApplicationDbContextFake.Create();
        var listId = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);
        var handler = new RenameTodoListCommandHandler(context);

        await Assert.ThrowsAsync<TodoListNameInvalidException>(
            () => handler.Handle(new RenameTodoListCommand(listId, ""), CancellationToken.None));
    }
}

using TodoApp.Application.TodoLists.Commands.CreateTodoList;
using TodoApp.Application.TodoLists.Queries.GetTodoLists;
using TodoApp.Application.UnitTests.Common;
using Xunit;

namespace TodoApp.Application.UnitTests.TodoLists.Queries.GetTodoLists;

public class GetTodoListsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithNoLists_ReturnsEmpty()
    {
        var context = ApplicationDbContextFake.Create();
        var handler = new GetTodoListsQueryHandler(context, new FakeCurrentUserService());

        var result = await handler.Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsAllListsOrderedByName()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Work"), CancellationToken.None);
        await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var result = await new GetTodoListsQueryHandler(context, currentUser).Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Equal(new[] { "Groceries", "Work" }, result.Select(l => l.Name));
    }

    [Fact]
    public async Task Handle_MapsIdAndName()
    {
        var context = ApplicationDbContextFake.Create();
        var currentUser = new FakeCurrentUserService();
        var id = await new CreateTodoListCommandHandler(context, currentUser).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var result = await new GetTodoListsQueryHandler(context, currentUser).Handle(new GetTodoListsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Groceries", dto.Name);
    }

    [Fact]
    public async Task Handle_DoesNotReturnAnotherUsersLists()
    {
        var context = ApplicationDbContextFake.Create();
        var owner = new FakeCurrentUserService("owner");
        var someoneElse = new FakeCurrentUserService("someone-else");
        await new CreateTodoListCommandHandler(context, owner).Handle(new CreateTodoListCommand("Owner's list"), CancellationToken.None);

        var result = await new GetTodoListsQueryHandler(context, someoneElse).Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}

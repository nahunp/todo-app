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
        var handler = new GetTodoListsQueryHandler(context);

        var result = await handler.Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsAllListsOrderedByName()
    {
        var context = ApplicationDbContextFake.Create();
        await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Work"), CancellationToken.None);
        await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var result = await new GetTodoListsQueryHandler(context).Handle(new GetTodoListsQuery(), CancellationToken.None);

        Assert.Equal(new[] { "Groceries", "Work" }, result.Select(l => l.Name));
    }

    [Fact]
    public async Task Handle_MapsIdAndName()
    {
        var context = ApplicationDbContextFake.Create();
        var id = await new CreateTodoListCommandHandler(context).Handle(new CreateTodoListCommand("Groceries"), CancellationToken.None);

        var result = await new GetTodoListsQueryHandler(context).Handle(new GetTodoListsQuery(), CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Groceries", dto.Name);
    }
}

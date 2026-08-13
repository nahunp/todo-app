using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;
using TodoApp.Domain.Exceptions;
using Xunit;

namespace TodoApp.Domain.UnitTests.Entities;

public class TodoListTests
{
    [Fact]
    public void Constructor_WithValidName_SetsName()
    {
        var list = new TodoList("Groceries");

        Assert.Equal("Groceries", list.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceName_Throws(string? name)
    {
        Assert.Throws<TodoListNameInvalidException>(() => new TodoList(name!));
    }

    [Fact]
    public void Constructor_WithNameOverMaxLength_Throws()
    {
        var tooLong = new string('a', 101);

        Assert.Throws<TodoListNameInvalidException>(() => new TodoList(tooLong));
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var list = new TodoList("  Groceries  ");

        Assert.Equal("Groceries", list.Name);
    }

    [Fact]
    public void Constructor_StartsWithNoItems()
    {
        var list = new TodoList("Groceries");

        Assert.Empty(list.Items);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var list = new TodoList("Original");

        list.Rename("Updated");

        Assert.Equal("Updated", list.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsAndLeavesOriginalNameUnchanged()
    {
        var list = new TodoList("Original");

        Assert.Throws<TodoListNameInvalidException>(() => list.Rename(""));
        Assert.Equal("Original", list.Name);
    }

    [Fact]
    public void AddItem_WithUniqueTitle_AddsItemToList()
    {
        var list = new TodoList("Groceries");

        list.AddItem("Buy milk");

        Assert.Single(list.Items);
        Assert.Equal("Buy milk", list.Items.Single().Title);
    }

    [Fact]
    public void AddItem_ReturnsTheCreatedItem()
    {
        var list = new TodoList("Groceries");

        var item = list.AddItem("Buy milk", priority: PriorityLevel.High);

        Assert.Equal("Buy milk", item.Title);
        Assert.Equal(PriorityLevel.High, item.Priority);
    }

    [Fact]
    public void AddItem_WithDuplicateTitle_Throws()
    {
        var list = new TodoList("Groceries");
        list.AddItem("Buy milk");

        Assert.Throws<DuplicateTodoItemTitleException>(() => list.AddItem("Buy milk"));
    }

    [Fact]
    public void AddItem_WithDuplicateTitleDifferentCaseAndWhitespace_Throws()
    {
        var list = new TodoList("Groceries");
        list.AddItem("Buy milk");

        Assert.Throws<DuplicateTodoItemTitleException>(() => list.AddItem("  BUY MILK  "));
    }

    [Fact]
    public void AddItem_WithDuplicateTitle_DoesNotAddItem()
    {
        var list = new TodoList("Groceries");
        list.AddItem("Buy milk");

        try
        {
            list.AddItem("Buy milk");
        }
        catch (DuplicateTodoItemTitleException)
        {
        }

        Assert.Single(list.Items);
    }

    [Fact]
    public void AddItem_WithInvalidTitle_PropagatesTodoItemTitleInvalidException()
    {
        var list = new TodoList("Groceries");

        Assert.Throws<TodoItemTitleInvalidException>(() => list.AddItem(""));
    }

    [Fact]
    public void RemoveItem_RemovesItemFromList()
    {
        var list = new TodoList("Groceries");
        var item = list.AddItem("Buy milk");

        list.RemoveItem(item);

        Assert.Empty(list.Items);
    }

    [Fact]
    public void RemoveItem_WithItemFromAnotherList_Throws()
    {
        var list = new TodoList("Groceries");
        var otherList = new TodoList("Work");
        var foreignItem = otherList.AddItem("Ship feature");

        Assert.Throws<TodoItemNotFoundInListException>(() => list.RemoveItem(foreignItem));
    }

    [Fact]
    public void RenameItem_WithUniqueTitle_RenamesItem()
    {
        var list = new TodoList("Groceries");
        var item = list.AddItem("Buy milk");

        list.RenameItem(item, "Buy oat milk");

        Assert.Equal("Buy oat milk", item.Title);
    }

    [Fact]
    public void RenameItem_WithDuplicateTitle_ThrowsAndLeavesOriginalTitleUnchanged()
    {
        var list = new TodoList("Groceries");
        list.AddItem("Buy milk");
        var item = list.AddItem("Buy bread");

        Assert.Throws<DuplicateTodoItemTitleException>(() => list.RenameItem(item, "Buy milk"));
        Assert.Equal("Buy bread", item.Title);
    }

    [Fact]
    public void RenameItem_ToItsOwnCurrentTitleWithDifferentCase_DoesNotThrow()
    {
        var list = new TodoList("Groceries");
        var item = list.AddItem("Buy milk");

        list.RenameItem(item, "BUY MILK");

        Assert.Equal("BUY MILK", item.Title);
    }

    [Fact]
    public void RenameItem_WithItemFromAnotherList_Throws()
    {
        var list = new TodoList("Groceries");
        var otherList = new TodoList("Work");
        var foreignItem = otherList.AddItem("Ship feature");

        Assert.Throws<TodoItemNotFoundInListException>(() => list.RenameItem(foreignItem, "Ship faster"));
    }
}

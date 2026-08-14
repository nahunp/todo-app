using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;
using TodoApp.Domain.Events;
using TodoApp.Domain.Exceptions;
using Xunit;

namespace TodoApp.Domain.UnitTests.Entities;

public class TodoItemTests
{
    [Fact]
    public void Constructor_WithValidTitle_SetsTitle()
    {
        var item = new TodoItem("Buy milk");

        Assert.Equal("Buy milk", item.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrWhitespaceTitle_Throws(string? title)
    {
        Assert.Throws<TodoItemTitleInvalidException>(() => new TodoItem(title!));
    }

    [Fact]
    public void Constructor_WithTitleOverMaxLength_Throws()
    {
        var tooLong = new string('a', 201);

        Assert.Throws<TodoItemTitleInvalidException>(() => new TodoItem(tooLong));
    }

    [Fact]
    public void Constructor_TrimsTitle()
    {
        var item = new TodoItem("  Buy milk  ");

        Assert.Equal("Buy milk", item.Title);
    }

    [Fact]
    public void Constructor_DefaultsPriorityToMedium()
    {
        var item = new TodoItem("Buy milk");

        Assert.Equal(PriorityLevel.Medium, item.Priority);
    }

    [Fact]
    public void Rename_WithValidTitle_UpdatesTitle()
    {
        var item = new TodoItem("Original");

        item.Rename("Updated");

        Assert.Equal("Updated", item.Title);
    }

    [Fact]
    public void Rename_WithEmptyTitle_ThrowsAndLeavesOriginalTitleUnchanged()
    {
        var item = new TodoItem("Original");

        Assert.Throws<TodoItemTitleInvalidException>(() => item.Rename(""));
        Assert.Equal("Original", item.Title);
    }

    [Fact]
    public void ChangePriority_UpdatesPriority()
    {
        var item = new TodoItem("Buy milk");

        item.ChangePriority(PriorityLevel.High);

        Assert.Equal(PriorityLevel.High, item.Priority);
    }

    [Fact]
    public void Constructor_DefaultsCategoryToNone()
    {
        var item = new TodoItem("Buy milk");

        Assert.Equal(TodoItemCategory.None, item.Category);
    }

    [Fact]
    public void SetCategory_UpdatesCategory()
    {
        var item = new TodoItem("Buy milk");

        item.SetCategory(TodoItemCategory.Work);

        Assert.Equal(TodoItemCategory.Work, item.Category);
    }

    [Fact]
    public void GetDueDateState_WithNoDueDate_ReturnsNone()
    {
        var item = new TodoItem("Buy milk");

        Assert.Equal(DueDateState.None, item.GetDueDateState(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void GetDueDateState_WithPastDueDate_ReturnsOverdue()
    {
        var now = new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);
        var item = new TodoItem("Buy milk", dueDate: now.AddDays(-1));

        Assert.Equal(DueDateState.Overdue, item.GetDueDateState(now));
    }

    [Fact]
    public void GetDueDateState_WithDueDateLaterTodaySameCalendarDay_ReturnsToday()
    {
        // Due date is later the same day as "now" — still Today, not
        // Overdue. This is the exact case the "compare Date, not the full
        // instant" comment on GetDueDateState is about.
        var now = new DateTimeOffset(2026, 8, 14, 8, 0, 0, TimeSpan.Zero);
        var item = new TodoItem("Buy milk", dueDate: new DateTimeOffset(2026, 8, 14, 23, 0, 0, TimeSpan.Zero));

        Assert.Equal(DueDateState.Today, item.GetDueDateState(now));
    }

    [Fact]
    public void GetDueDateState_WithFutureDueDate_ReturnsUpcoming()
    {
        var now = new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);
        var item = new TodoItem("Buy milk", dueDate: now.AddDays(3));

        Assert.Equal(DueDateState.Upcoming, item.GetDueDateState(now));
    }

    [Fact]
    public void MarkComplete_SetsIsDoneAndCompletedAtTogether()
    {
        var item = new TodoItem("Buy milk");

        item.MarkComplete();

        Assert.True(item.IsDone);
        Assert.NotNull(item.CompletedAt);
    }

    [Fact]
    public void MarkComplete_WhenAlreadyDone_IsNoOpAndKeepsOriginalCompletedAt()
    {
        var item = new TodoItem("Buy milk");
        item.MarkComplete();
        var firstCompletedAt = item.CompletedAt;

        item.MarkComplete();

        Assert.Equal(firstCompletedAt, item.CompletedAt);
    }

    [Fact]
    public void MarkComplete_RaisesTodoItemCompletedEvent()
    {
        var item = new TodoItem("Buy milk");

        item.MarkComplete();

        var domainEvent = Assert.Single(item.DomainEvents);
        Assert.IsType<TodoItemCompletedEvent>(domainEvent);
    }

    [Fact]
    public void MarkComplete_WhenAlreadyDone_DoesNotRaiseASecondEvent()
    {
        var item = new TodoItem("Buy milk");
        item.MarkComplete();

        item.MarkComplete();

        Assert.Single(item.DomainEvents);
    }

    [Fact]
    public void Reopen_ClearsIsDoneAndCompletedAt()
    {
        var item = new TodoItem("Buy milk");
        item.MarkComplete();

        item.Reopen();

        Assert.False(item.IsDone);
        Assert.Null(item.CompletedAt);
    }

    [Fact]
    public void Reopen_WhenNotDone_IsNoOp()
    {
        var item = new TodoItem("Buy milk");

        item.Reopen();

        Assert.False(item.IsDone);
        Assert.Null(item.CompletedAt);
    }
}

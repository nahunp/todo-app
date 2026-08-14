using TodoApp.Domain.Common;
using TodoApp.Domain.Enums;
using TodoApp.Domain.Events;
using TodoApp.Domain.Exceptions;

namespace TodoApp.Domain.Entities;

public class TodoItem : BaseAuditableEntity
{
    private const int TitleMaxLength = 200;

    public string Title { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public bool IsDone { get; private set; }

    /// <summary>
    /// Null while the item is open. Set the instant it's completed, cleared
    /// on reopen. Kept in lockstep with IsDone by MarkComplete/Reopen only —
    /// there's no public setter for either, so the two can never disagree.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    public PriorityLevel Priority { get; private set; } = PriorityLevel.Medium;

    public DateTimeOffset? DueDate { get; private set; }

    public TodoItemCategory Category { get; private set; } = TodoItemCategory.None;

    // EF Core needs a parameterless constructor to materialize entities from
    // the database, but we don't want application code calling `new TodoItem()`
    // and getting an invalid, title-less item. `private` satisfies EF (it uses
    // reflection, so accessibility doesn't matter to it) while keeping the
    // public API honest: the only way to get a valid TodoItem is through the
    // constructor below.
    private TodoItem()
    {
    }

    public TodoItem(string title, string? notes = null, PriorityLevel priority = PriorityLevel.Medium, DateTimeOffset? dueDate = null, TodoItemCategory category = TodoItemCategory.None)
    {
        SetTitle(title);
        Notes = notes;
        Priority = priority;
        DueDate = dueDate;
        Category = category;
    }

    public void Rename(string title) => SetTitle(title);

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new TodoItemTitleInvalidException("Title cannot be empty.");

        title = title.Trim();

        if (title.Length > TitleMaxLength)
            throw new TodoItemTitleInvalidException($"Title cannot exceed {TitleMaxLength} characters.");

        Title = title;
    }

    public void UpdateNotes(string? notes) => Notes = notes;

    public void ChangePriority(PriorityLevel priority) => Priority = priority;

    public void SetDueDate(DateTimeOffset? dueDate) => DueDate = dueDate;

    public void SetCategory(TodoItemCategory category) => Category = category;

    /// <summary>
    /// Overdue/Today/Upcoming relative to <paramref name="asOf"/>, not
    /// DateTimeOffset.UtcNow directly — a pure function of its input is
    /// trivially testable without needing a clock abstraction this codebase
    /// doesn't otherwise have (see TodoItemCompletedEvent/MarkComplete,
    /// which do call UtcNow directly; that's fine for "record the instant
    /// this happened," but this is a read-time projection, not a fact being
    /// recorded, so the caller supplies "now"). Compares calendar dates,
    /// not instants — a due date of "today at midnight" shouldn't flip to
    /// Overdue five minutes later just because time passed within the same
    /// day.
    /// </summary>
    public DueDateState GetDueDateState(DateTimeOffset asOf)
    {
        if (DueDate is null)
            return DueDateState.None;

        var due = DueDate.Value.Date;
        var today = asOf.Date;

        if (due < today) return DueDateState.Overdue;
        if (due == today) return DueDateState.Today;
        return DueDateState.Upcoming;
    }

    public void MarkComplete()
    {
        if (IsDone)
            return; // already done — a no-op, not an error. See PR description for why.

        IsDone = true;
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TodoItemCompletedEvent(this));
    }

    public void Reopen()
    {
        if (!IsDone)
            return;

        IsDone = false;
        CompletedAt = null;
    }
}

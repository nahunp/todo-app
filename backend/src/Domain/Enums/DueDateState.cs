namespace TodoApp.Domain.Enums;

/// <summary>
/// Derived from TodoItem.DueDate at read time, never stored — "overdue"
/// and "today" are relative to when you ask, not a fact about the row.
/// See TodoItem.GetDueDateState.
/// </summary>
public enum DueDateState
{
    None = 0,
    Overdue = 1,
    Today = 2,
    Upcoming = 3
}

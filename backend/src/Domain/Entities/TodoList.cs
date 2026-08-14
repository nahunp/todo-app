using TodoApp.Domain.Common;
using TodoApp.Domain.Enums;
using TodoApp.Domain.Exceptions;

namespace TodoApp.Domain.Entities;

/// <summary>
/// The aggregate root for a group of TodoItems. "Aggregate root" means this
/// is the ONLY supported entry point for any mutation that has to see every
/// item in the list at once — right now that's just "no two items in the
/// same list share a title" — instead of trusting every call site elsewhere
/// in the codebase to remember and re-check that rule.
///
/// TodoItem still owns its own invariants (title non-empty/length,
/// IsDone/CompletedAt) exactly as before — that didn't change. What
/// TodoList adds on top is the invariant that only makes sense at the list
/// level: no duplicate titles *within this list*. A standalone TodoItem has
/// no way to know about its siblings, so that check has to live here.
///
/// Note: RenameItem/RemoveItem take the TodoItem instance itself, not an
/// int id. TodoItem.Id is DB-assigned (EF Core identity column) and stays 0
/// until SaveChanges runs — so right after AddItem, in the same unit of
/// work, every item in a list would still have Id == 0 and an id-based
/// lookup couldn't tell them apart. Taking the reference works whether the
/// list has been persisted yet or not, and the item is already right there
/// in AddItem's return value / the Items collection.
/// </summary>
public class TodoList : BaseAuditableEntity
{
    private const int NameMaxLength = 100;

    private readonly List<TodoItem> _items = new();

    /// <summary>
    /// The Identity user id (string, matching IdentityUser.Id) that owns
    /// this list. Set once at construction, never changed — lists don't
    /// transfer ownership. Not validated the same way as Name: an empty
    /// OwnerId means the *caller* (Application layer) has a bug (it should
    /// always come from an authenticated ICurrentUserService, never
    /// straight from user input), not a business rule a real user could
    /// trip — hence ArgumentException, not a DomainException.
    /// </summary>
    public string OwnerId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<TodoItem> Items => _items.AsReadOnly();

    // Same reasoning as TodoItem's private parameterless constructor: EF Core
    // needs it to materialize entities, application code doesn't get to call it.
    private TodoList()
    {
    }

    public TodoList(string ownerId, string name)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));

        OwnerId = ownerId;
        SetName(name);
    }

    public void Rename(string name) => SetName(name);

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new TodoListNameInvalidException("Name cannot be empty.");

        name = name.Trim();

        if (name.Length > NameMaxLength)
            throw new TodoListNameInvalidException($"Name cannot exceed {NameMaxLength} characters.");

        Name = name;
    }

    /// <summary>
    /// Creates a new TodoItem and adds it to this list. This is the only way
    /// items get into a TodoList — there's no AddItem(TodoItem) overload, on
    /// purpose, so an item can't be constructed standalone somewhere else and
    /// then dropped into two different lists behind the aggregate's back.
    /// </summary>
    public TodoItem AddItem(string title, string? notes = null, PriorityLevel priority = PriorityLevel.Medium, DateTimeOffset? dueDate = null, TodoItemCategory category = TodoItemCategory.None)
    {
        EnsureTitleIsUnique(title);

        var item = new TodoItem(title, notes, priority, dueDate, category); // TodoItem still validates/trims its own title
        _items.Add(item);
        return item;
    }

    public void RemoveItem(TodoItem item)
    {
        EnsureItemBelongsToList(item);
        _items.Remove(item);
    }

    /// <summary>
    /// Renames an item that belongs to this list, checking the cross-item
    /// uniqueness rule before handing off to TodoItem.Rename for its own
    /// (empty/length) validation. Deliberately routed through the list
    /// rather than calling item.Rename(...) directly — TodoItem.Rename is
    /// still public (needed so it stays independently testable, see
    /// TodoItemTests), so nothing at the compiler level stops someone from
    /// bypassing this check. That's a convention this codebase follows, not
    /// something C# enforces: Application-layer command handlers call
    /// TodoList.RenameItem, never item.Rename directly, once an item
    /// belongs to a list.
    /// </summary>
    public void RenameItem(TodoItem item, string newTitle)
    {
        EnsureItemBelongsToList(item);
        EnsureTitleIsUnique(newTitle, excluding: item);

        item.Rename(newTitle);
    }

    private void EnsureItemBelongsToList(TodoItem item)
    {
        if (!_items.Contains(item))
            throw new TodoItemNotFoundInListException();
    }

    private void EnsureTitleIsUnique(string title, TodoItem? excluding = null)
    {
        var candidate = title?.Trim();

        var isDuplicate = _items.Any(i =>
            !ReferenceEquals(i, excluding) &&
            string.Equals(i.Title, candidate, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
            throw new DuplicateTodoItemTitleException(candidate ?? string.Empty);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // TodoItem has no DbSet of its own (only reachable via
        // TodoList.Items), so EF Core's table-naming convention falls back
        // to the CLR type name — "TodoItem", singular — instead of
        // matching "TodoLists" (from the DbSet property name). Explicit,
        // for consistency.
        builder.ToTable("TodoItems");

        // Matches TodoItem's own TitleMaxLength constant.
        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        // Notes is intentionally left unconstrained (TodoItem itself puts
        // no limit on it) — nvarchar(max) is the right default here, not
        // an oversight.

        // Backstop for TodoList.EnsureTitleIsUnique — that check is an
        // in-memory scan of the already-loaded Items collection, which is
        // exactly the kind of thing two genuinely concurrent requests can
        // both pass before either commits (found live: a rapid double-click
        // on "Add" fired two AddTodoItem requests with the same title;
        // both loaded the list before either had saved, both saw no
        // existing duplicate, both inserted — TodoListTests'
        // "AddItem_WithDuplicateTitle_Throws" only covers the single-
        // request case). "TodoListId" is TodoItem's shadow FK property
        // (see TodoListConfiguration's HasMany().WithOne() comment) —
        // referenced by string since there's no CLR property for it. The
        // database's collation (SQL Server default here is case-
        // insensitive) makes this match TodoList's own OrdinalIgnoreCase
        // comparison closely enough in practice.
        builder.HasIndex("TodoListId", nameof(TodoItem.Title))
            .IsUnique();
    }
}

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
    }
}

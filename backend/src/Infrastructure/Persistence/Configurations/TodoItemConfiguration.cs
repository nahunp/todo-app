using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Matches TodoItem's own TitleMaxLength constant.
        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        // Notes is intentionally left unconstrained (TodoItem itself puts
        // no limit on it) — nvarchar(max) is the right default here, not
        // an oversight.
    }
}

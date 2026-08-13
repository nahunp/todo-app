using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence.Configurations;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        // Matches TodoList.NameMaxLength — keeps the column from defaulting
        // to nvarchar(max) for something the domain already caps at 100.
        builder.Property(l => l.Name)
            .HasMaxLength(100)
            .IsRequired();

        // TodoList.Items is IReadOnlyCollection<TodoItem> over a private
        // field (_items), by design — see TodoList.cs. EF Core supports
        // this encapsulated-collection pattern, but has to be told which
        // field backs the navigation and to use it directly (bypassing the
        // read-only property) when materializing/tracking. No FK is
        // exposed on TodoItem on purpose; WithOne() with no argument tells
        // EF Core to manage the foreign key itself, as a shadow property.
        //
        // Same configuration as ApplicationDbContextFake in
        // TodoApp.Application.UnitTests — duplicated there deliberately so
        // the test project doesn't have to depend on Infrastructure.
        builder.HasMany(l => l.Items)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Identity;

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

        // 450, matching ASP.NET Core Identity's own convention for
        // Id/UserName columns (kept under SQL Server's 900-byte composite
        // index key limit). OwnerId is a plain string on the Domain side
        // (TodoList doesn't reference ApplicationUser - that's an
        // Infrastructure/Identity type), but there's still a real FK here
        // at the database level: configured from this side since
        // ApplicationUser has no reciprocal "Lists" navigation, same
        // reasoning as the Items relationship below. Cascade: deleting a
        // user deletes their lists (and, transitively, their items) rather
        // than leaving orphaned rows.
        builder.Property(l => l.OwnerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(l => l.OwnerId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

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
        // IsRequired(): a TodoItem only ever comes into existence via
        // TodoList.AddItem, so the shadow TodoListId FK should be NOT
        // NULL, not the EF default (nullable/optional) for a WithOne()
        // with no reciprocal navigation.
        builder.HasMany(l => l.Items)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

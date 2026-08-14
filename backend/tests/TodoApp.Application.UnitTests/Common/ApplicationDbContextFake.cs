using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.UnitTests.Common;

/// <summary>
/// A real EF Core DbContext, backed by the InMemory provider, standing in
/// for IApplicationDbContext in tests. No mocking library involved —
/// DbSet&lt;T&gt; is awkward to mock well (it's IQueryable, not just a
/// list), so handlers get exercised against real LINQ/SaveChanges behaviour
/// instead. Each instance gets its own uniquely-named in-memory database
/// (see Create()), so tests never see each other's data, even run in
/// parallel.
/// </summary>
public class ApplicationDbContextFake : DbContext, IApplicationDbContext
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();

    private ApplicationDbContextFake(DbContextOptions<ApplicationDbContextFake> options)
        : base(options)
    {
    }

    public static ApplicationDbContextFake Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContextFake>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContextFake(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>(list =>
        {
            // TodoList.Items is IReadOnlyCollection<TodoItem> over a private
            // field (_items) — see TodoList.cs, this is deliberate
            // encapsulation, not an oversight. EF Core supports this
            // pattern, but has to be told which field backs the navigation
            // and to use that field directly, bypassing the read-only
            // property, when materializing/tracking. No FK is exposed on
            // TodoItem on purpose; WithOne() with no argument tells EF Core
            // to manage the foreign key itself, as a shadow property.
            //
            // Once Infrastructure exists with a real DbContext, this
            // configuration belongs there too — duplicated here for now
            // since this fake is the only DbContext in the solution.
            list.HasMany(l => l.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            list.Navigation(l => l.Items)
                .HasField("_items")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}

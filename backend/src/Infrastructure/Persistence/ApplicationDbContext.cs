using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Persistence;

/// <summary>
/// The real IApplicationDbContext, backed by SQL Server. Application-layer
/// handlers never see this type directly — they only know
/// IApplicationDbContext — so nothing above this layer has to change if the
/// database ever did (SQLite for local dev, a different provider, etc.).
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in this assembly —
        // TodoListConfiguration, TodoItemConfiguration, and whatever gets
        // added later — instead of listing them here by hand.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

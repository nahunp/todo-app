using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Domain.Entities;
using TodoApp.Infrastructure.Identity;

namespace TodoApp.Infrastructure.Persistence;

/// <summary>
/// The real IApplicationDbContext, backed by SQL Server. Application-layer
/// handlers never see this type directly — they only know
/// IApplicationDbContext — so nothing above this layer has to change if the
/// database ever did (SQLite for local dev, a different provider, etc.).
///
/// IdentityDbContext&lt;ApplicationUser&gt;, not plain DbContext — this is
/// also the store for Identity's own tables (AspNetUsers, AspNetRoles,
/// etc.). One database, one DbContext; no reason to split them for an app
/// this size.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base first — sets up Identity's own tables/relationships. Our
        // configs never touch those, but this is the conventional order for
        // IdentityDbContext and there's no reason to deviate from it.
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly —
        // TodoListConfiguration, TodoItemConfiguration, and whatever gets
        // added later — instead of listing them here by hand.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Common.Interfaces;

/// <summary>
/// Everything Application-layer handlers need from persistence, expressed as
/// an interface so handlers can be unit tested without a real database and
/// so Application never references a specific EF Core provider (SqlServer,
/// Sqlite, ...) — that choice belongs to Infrastructure, which will implement
/// this interface on the real DbContext.
///
/// Application DOES take a dependency on the core Microsoft.EntityFrameworkCore
/// package here, just for the DbSet&lt;T&gt; type — that's a deliberate,
/// common trade-off in this style of Clean Architecture (not the "zero
/// dependencies" rule Domain follows). DbSet gives handlers real LINQ
/// composition (.Include(), .Where(), etc.) directly against the interface;
/// modeling that through a hand-rolled repository method for every query
/// shape would either balloon the interface or leak the same EF concepts
/// anyway. No actual database provider is referenced here — just the
/// abstraction.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

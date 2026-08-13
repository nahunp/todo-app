using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TodoApp.Infrastructure.Persistence;

/// <summary>
/// Lets EF Core's tooling (Add-Migration / Update-Database in Package
/// Manager Console, or `dotnet ef` on the CLI) construct an
/// ApplicationDbContext at design time. Normally that tooling asks the
/// application's own startup (Program.cs, reading appsettings.json) to
/// build the DbContext — but there's no Web API/host project yet, so this
/// factory is the bridge until one exists. Only used by the tooling, never
/// by the running app itself.
///
/// Default (unnamed) local SQL Server instance, Windows-integrated auth,
/// hardcoded here on purpose — once the Web API project exists, it'll read
/// the real connection string from configuration at runtime via
/// DependencyInjection.AddInfrastructureServices, and this factory's job
/// shrinks to just design-time tooling support.
///
/// If SSMS connects with a SQL login (username/password) instead of
/// Windows Authentication, swap Trusted_Connection=True for
/// `User Id=...;Password=...;` — don't commit a real password here, this
/// file is design-time-tooling-only but it's still source control.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=TodoAppDb;Trusted_Connection=True;TrustServerCertificate=True");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

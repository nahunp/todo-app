using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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
/// The connection string is NOT hardcoded here — it's read from .NET User
/// Secrets (see TodoApp.Infrastructure.csproj's UserSecretsId), a
/// secrets.json file that lives outside the repo entirely
/// (%APPDATA%\Microsoft\UserSecrets\&lt;id&gt;\secrets.json on Windows), set via
/// Visual Studio: right-click TodoApp.Infrastructure -> Manage User
/// Secrets. Nothing sensitive ever touches source control this way, even
/// once real secrets (SQL auth, Azure connection strings) show up later.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ApplicationDbContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. In Visual Studio: " +
                "right-click TodoApp.Infrastructure -> Manage User Secrets, then add " +
                "{ \"ConnectionStrings\": { \"DefaultConnection\": \"...\" } }.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.Infrastructure;

/// <summary>
/// Registers the real, SQL-Server-backed IApplicationDbContext. The (future)
/// Web API project calls this once at startup, alongside
/// AddApplicationServices() — same one-call-per-layer shape.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        // Handlers depend on IApplicationDbContext, never ApplicationDbContext
        // directly — this is the one place that connects the two.
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}

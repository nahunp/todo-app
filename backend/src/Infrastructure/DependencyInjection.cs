using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Infrastructure.Identity;
using TodoApp.Infrastructure.Persistence;

namespace TodoApp.Infrastructure;

/// <summary>
/// Registers the real, SQL-Server-backed IApplicationDbContext plus
/// Identity. The (future) Web API project calls this once at startup,
/// alongside AddApplicationServices() — same one-call-per-layer shape.
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

        // AddIdentityCore, not AddIdentity — this is a JWT-bearer-only API,
        // no cookie-based sign-in and no roles yet. AddIdentity registers a
        // cookie auth scheme as a side effect, which fights with JWT Bearer
        // over which one's the default (a common, confusing source of "API
        // call gets a 302 redirect instead of a 401"). AddIdentityCore
        // avoids that whole class of problem by not registering it at all.
        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}

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

        // EnableRetryOnFailure: Azure SQL's serverless free tier auto-pauses
        // after a period of no activity and takes tens of seconds to resume.
        // A request arriving during that resume can hit a transient error
        // (Error 40613, "not currently available") that EF Core's default
        // execution strategy doesn't retry - it surfaces as an unhandled
        // exception (500), even though the underlying command frequently
        // still completes moments later once the database finishes waking
        // up (confirmed live: a DELETE that 500'd had actually deleted the
        // row by the time the response came back). Local SQL Server Express
        // has no such pause behavior, so this is a no-op there.
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

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

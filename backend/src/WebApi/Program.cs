using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Application;
using TodoApp.Application.Common.Interfaces;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Persistence;
using TodoApp.WebApi.Auth;
using TodoApp.WebApi.Common;
using TodoApp.WebApi.Identity;
using TodoApp.WebApi.TodoLists;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// One call per layer — each layer owns registering its own services.
// See DependencyInjection.cs in Application and Infrastructure.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Lets Swagger UI's "Authorize" button attach a Bearer token to every
    // request after logging in via /api/v1/auth/login — otherwise every
    // manual test through the UI would need a separate tool just to add
    // the header.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste just the token — Swagger adds the \"Bearer \" prefix itself.",
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

// Frontend is always a different origin from this API — localhost:4200 in
// dev (ng serve), the deployed Static Web App's *.azurestaticapps.net
// origin in production — so the browser blocks requests without an
// explicit CORS policy either way. Origins come from configuration
// (Cors:AllowedOrigins, see appsettings.json for the dev default; Azure
// overrides it via the Cors__AllowedOrigins__0 app setting) rather than
// being hardcoded, so a new deployed frontend origin is a config change,
// not a code change. Scoped to specific origins, not AllowAnyOrigin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it in User Secrets (same secrets.json as the connection string).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // See CurrentUserService's doc comment — without this, "sub" gets
        // silently rewritten to a long ClaimTypes URI and UserId lookups
        // quietly return null.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            // Default is 5 minutes; tokens here are already short-lived
            // (60 min, see TokenService), no need for a generous allowance.
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Convenience for local dev only: applies any pending migrations on
    // startup so `dotnet run` always reflects the latest schema without a
    // manual `dotnet ef database update` first. NOT how you'd want this in
    // production — migrations there run as an explicit deploy step
    // (`dotnet ef database update` against the target connection string),
    // not implicitly on every instance's startup (races if more than one
    // instance starts at once, no chance to review the SQL first).
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Must come before endpoint mapping so it can catch everything downstream.
app.UseExceptionHandler();

app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapTodoListEndpoints();

app.Run();

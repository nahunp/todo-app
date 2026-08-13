using Microsoft.EntityFrameworkCore;
using TodoApp.Application;
using TodoApp.Infrastructure;
using TodoApp.Infrastructure.Persistence;
using TodoApp.WebApi.Common;
using TodoApp.WebApi.TodoLists;

const string FrontendDevCorsPolicy = "FrontendDev";

var builder = WebApplication.CreateBuilder(args);

// One call per layer — each layer owns registering its own services.
// See DependencyInjection.cs in Application and Infrastructure.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Angular's dev server (`ng serve`) runs on its own origin (localhost:4200)
// — different port from this API (5080), so the browser blocks requests
// between them without an explicit CORS policy. Scoped to exactly that
// origin, not AllowAnyOrigin — this policy is meaningless as a security
// boundary once there's a real deployed frontend origin, but that's a
// problem for whenever deployment actually happens, not now.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendDevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(FrontendDevCorsPolicy);

    // Convenience for local dev only: applies any pending migrations on
    // startup so `dotnet run` always reflects the latest schema without a
    // manual `dotnet ef database update` first. NOT how you'd want this in
    // production — migrations there should run as an explicit deploy step,
    // not implicitly on every instance's startup (races if more than one
    // instance starts at once, no chance to review the SQL first).
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Must come before endpoint mapping so it can catch everything downstream.
app.UseExceptionHandler();

app.MapTodoListEndpoints();

app.Run();

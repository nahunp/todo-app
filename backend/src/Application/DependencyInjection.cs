using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Common.Behaviours;

namespace TodoApp.Application;

/// <summary>
/// Registers everything this layer owns — MediatR handlers, FluentValidation
/// validators, and the ValidationBehaviour that connects them — in one place.
/// The (future) Web API project just calls AddApplicationServices() once at
/// startup; it doesn't need to know MediatR or FluentValidation are involved.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}

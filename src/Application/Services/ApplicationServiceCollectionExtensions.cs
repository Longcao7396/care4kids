using System.Reflection;
using FluentValidation;
using GiveAID.Application.Common.Behaviors;
using GiveAID.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GiveAID.Application.Services;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Application layer services including MediatR and FluentValidation.
    /// M-02 FIX: ValidationBehavior is now registered to automatically validate all requests.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR from the Application assembly
        var applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            // M-02 FIX: Register ValidationBehavior as a pipeline behavior
            // This will automatically run FluentValidation validators before handlers execute
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register FluentValidation validators from the Application assembly
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}

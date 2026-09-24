using System.Reflection;
using FluentValidation;
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
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR from the Application assembly
        var applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // Register FluentValidation validators from the Application assembly
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}

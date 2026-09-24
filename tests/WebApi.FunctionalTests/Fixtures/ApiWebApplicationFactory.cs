using System.Collections.Generic;
using GiveAID.Application.Common.Interfaces;
using GiveAID.Infrastructure.Persistence;
using GiveAID.V2.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GiveAID.Tests.Integration.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"GiveAIDTest_{Guid.NewGuid()}";

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKeyForJwtTokenGenerationMustBeAtLeast64BytesLong-2024",
                ["Jwt:Issuer"] = "GiveAID.Test",
                ["Jwt:Audience"] = "GiveAID.Test.Client",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Email:From"] = "test@example.com",
                ["Email:SmtpHost"] = "localhost",
                ["Email:SmtpPort"] = "25",
                ["Stripe:SecretKey"] = "sk_test_mock",
                ["Stripe:PublishableKey"] = "pk_test_mock",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbName}.db"
            });
        });

        builder.UseEnvironment("Testing");

        // ConfigureWebHost on this instance runs via base.CreateHost
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove concrete DbContext-derived type registrations only (type-exact, not string-contains)
            // DO NOT remove IApplicationDbContext interface mapping here — it will be re-added below
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<GiveAIDDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(GiveAIDDbContext)).ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            // Also remove any Microsoft.EntityFrameworkCore.* services
            var efServices = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                .ToList();
            foreach (var efService in efServices)
            {
                services.Remove(efService);
            }

            // Add InMemory database
            services.AddDbContext<GiveAIDDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Re-add IApplicationDbContext mapping so controllers receive the concrete DbContext
            // Uses Scoped lifetime to match AddInfrastructureServices registration
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<GiveAIDDbContext>());
        });
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

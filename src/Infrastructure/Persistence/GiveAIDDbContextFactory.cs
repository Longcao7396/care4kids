using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GiveAID.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can build GiveAIDDbContext
/// without requiring the WebApi project to be the startup project (avoids the
/// WebApi bin/ file lock while `dotnet watch run` is active). This is only
/// used by the EF Core CLI tooling — never invoked at application runtime.
/// </summary>
public class GiveAIDDbContextFactory : IDesignTimeDbContextFactory<GiveAIDDbContext>
{
    public GiveAIDDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WebApi");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath(basePath))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=GiveAIDDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connect Timeout=15";

        var optionsBuilder = new DbContextOptionsBuilder<GiveAIDDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new GiveAIDDbContext(optionsBuilder.Options);
    }
}

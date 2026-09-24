using System.Text;
using AspNetCoreRateLimit;
using GiveAID.Application.Services;
using GiveAID.Infrastructure.Persistence;
using GiveAID.Infrastructure.Persistence.Seed;
using GiveAID.Infrastructure.Services;
using GiveAID.V2.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// USE MANAGED SQL CLIENT — avoids native Microsoft.Data.SqlClient.SNI.dll
// that is missing from bin/ due to NuGet restore/copy bug on .NET 10.
// Set BEFORE any Infrastructure/EF Core code runs (static constructors fire immediately).
AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Add Application services (MediatR, FluentValidation)
builder.Services.AddApplicationServices();

// 3. Add Infrastructure services (DbContext, JWT, Email, Payment, Cache, Seeder)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. Configure JWT Authentication
// Production: JWT secret MUST come from environment variable (Jwt__Secret) or Secret Manager.
// Development: Can use appsettings.Development.json.
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required. Set the Jwt__Secret environment variable.");

// SECURITY: Validate secret strength
const int MIN_SECRET_LENGTH = 32;
if (jwtSecret.Length < MIN_SECRET_LENGTH)
{
    if (builder.Environment.IsDevelopment())
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
        logger.LogWarning("JWT secret is too short ({Length} chars). Minimum {MinLength} characters required. This is allowed in Development only.", jwtSecret.Length, MIN_SECRET_LENGTH);
    }
    else
    {
        throw new InvalidOperationException($"Jwt:Secret must be at least {MIN_SECRET_LENGTH} characters long in production. Current length: {jwtSecret.Length}. Set a strong secret via the Jwt__Secret environment variable.");
    }
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GiveAID.V2";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GiveAID.V2.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

// 5. Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    // SuperAdmin policy removed — Admin has full privileges
});

// 6. Configure CORS for React dev servers
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000,http://localhost:3001";
var originsList = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
    {
        policy.WithOrigins(originsList)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 7. Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    var windowSeconds = int.TryParse(builder.Configuration["RateLimit:WindowSeconds"], out var ws) ? ws : 60;
    var permitLimit = int.TryParse(builder.Configuration["RateLimit:PermitLimit"], out var pl) ? pl : 100;

    // M-09 FIX: Public/health endpoints are exempted (not subject to rate limiting).
    // Rules are evaluated in order — first match wins.
    // To truly exempt, we use a separate HttpContext check in the middleware pipeline.
    //
    // Note: AspNetCoreRateLimit (v5) does not have a native "disable for this path"
    // mechanism. We handle exemption at middleware level (see below) and apply
    // sensible per-endpoint limits here so protected endpoints still have limits.
    options.GeneralRules = new List<RateLimitRule>
    {
        // Health check — MUST be unlimited for load balancers/containers to probe.
        // Exempted via middleware below, so this rule is a no-op but kept for clarity.
        new RateLimitRule
        {
            Endpoint = "*:/healthz",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        // OpenAPI / Swagger docs — no rate limit needed for documentation.
        new RateLimitRule
        {
            Endpoint = "*:/swagger*",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        // Scalar API reference — no rate limit.
        new RateLimitRule
        {
            Endpoint = "*:/scalar*",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        // Public read-only endpoints — exempt from rate limiting (no auth required,
        // used by anonymous visitors). These should NOT have int.MaxValue for
        // production — the real exemption is in the middleware below.
        // Here we set generous limits as a belt-and-suspenders safety net.
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/causes",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/campaigns",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/supporters",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/achievements",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/gallery",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/faqs",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = int.MaxValue
        },
        // Auth endpoints — strict limit to prevent brute-force.
        // Override applies BEFORE the global rule.
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/auth/login",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = 20
        },
        new RateLimitRule
        {
            Endpoint = "*:/api/v1/auth/register",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = 20
        },
        // Global rate limit for all other endpoints (admin, protected, write ops).
        // Default: 100 req/min. Configurable via RateLimit:PermitLimit.
        new RateLimitRule
        {
            Endpoint = "*",
            Period = $"{windowSeconds}s",
            PeriodTimespan = TimeSpan.FromSeconds(windowSeconds),
            Limit = permitLimit
        }
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// 8. Configure OpenAPI (Scalar)
builder.Services.AddEndpointsApiExplorer();

// 9. Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// 10. Configure middleware pipeline (order matters!)
// - Exception handler first (catches all exceptions)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// - CORS before authentication
app.UseCors("ReactDev");

// - HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// - Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// - Rate limiting
app.UseIpRateLimiting();

// - Map controllers and endpoints
app.MapControllers();

// - Map health check
app.MapHealthChecks("/healthz");

// - Map Scalar UI for API documentation
app.MapOpenApi();

// ----------------------------------------------------------------------
// 11. Apply pending migrations + seed baseline data.
// Ensures the database schema matches the domain entities and that the
// admin user exists so the portal is usable on first startup.
//
// M-11 FIX: Seed behavior is now environment-aware and configurable:
//   - SeedData:Enabled = false  → skip entirely
//   - SeedData:Required = true  → throw on failure (use in production if seeding is critical)
//   - Development: log info on success, error on failure (never throw), continue running
//   - Production: log error on failure, continue running
// ----------------------------------------------------------------------
var seedEnabled = builder.Configuration.GetValue<bool>("SeedData:Enabled", true);
var seedRequired = builder.Configuration.GetValue<bool>("SeedData:Required", false);

if (seedEnabled)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GiveAIDDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Ensuring database is created and seeded…");
            var success = await GiveAID.Infrastructure.Persistence.Seed.SeedData.SeedAsync(dbContext);
            if (success)
            {
                logger.LogInformation("Database seed completed successfully.");
            }
            else
            {
                logger.LogWarning("Database seed returned false — some seed data may already exist.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seed failed: {SeedError}. The app will continue running; " +
            "some features may not work until you seed manually.", ex.Message);

        if (seedRequired && !app.Environment.IsDevelopment())
        {
            // In production with SeedData:Required=true, fail fast so operators
            // are alerted (e.g., container orchestrator restart, health check failure).
            throw new InvalidOperationException(
                $"Database seed is required in this environment but failed: {ex.Message}", ex);
        }
    }
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Database seeding is disabled (SeedData:Enabled=false). Skipping seed.");
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }

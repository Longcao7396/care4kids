using GiveAID.Application.Features.Health;
using MediatR;

namespace GiveAID.Application.Features.Health.Queries;

/// <summary>
/// Handler for HealthCheckQuery.
/// </summary>
public class HealthCheckQueryHandler : IRequestHandler<HealthCheckQuery, HealthCheckDto>
{
    public Task<HealthCheckDto> Handle(HealthCheckQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HealthCheckDto
        {
            Status = "Healthy",
            Version = "2.0.0",
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, string>
            {
                { "Application", "Healthy" },
                { "Database", "Healthy" },
                { "Cache", "Healthy" }
            }
        });
    }
}

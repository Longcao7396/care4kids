using GiveAID.Application.Features.Health;
using MediatR;

namespace GiveAID.Application.Features.Health.Queries;

/// <summary>
/// Query for health check.
/// </summary>
public class HealthCheckQuery : IRequest<HealthCheckDto>
{
}

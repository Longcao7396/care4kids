using GiveAID.Application.Features.Careers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Careers.Queries.GetActive;

/// <summary>
/// Query to get active careers.
/// </summary>
public class GetActiveCareersQuery : IRequest<IEnumerable<CareerDto>>
{
}

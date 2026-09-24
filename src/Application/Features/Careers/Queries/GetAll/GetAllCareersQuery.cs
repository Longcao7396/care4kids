using GiveAID.Application.Features.Careers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Careers.Queries.GetAll;

/// <summary>
/// Query to get all careers.
/// </summary>
public class GetAllCareersQuery : IRequest<IEnumerable<CareerDto>>
{
    public bool ActiveOnly { get; set; } = false;
}

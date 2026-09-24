using GiveAID.Application.Features.Causes.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Causes.Queries.GetAll;

/// <summary>
/// Query to get all causes.
/// </summary>
public class GetAllCausesQuery : IRequest<IEnumerable<CauseDto>>
{
    public bool ActiveOnly { get; set; } = true;
}

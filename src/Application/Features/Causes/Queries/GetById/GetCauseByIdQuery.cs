using GiveAID.Application.Features.Causes.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Causes.Queries.GetById;

/// <summary>
/// Query to get a cause by ID.
/// </summary>
public class GetCauseByIdQuery : IRequest<CauseDto>
{
    public int CauseId { get; set; }
}

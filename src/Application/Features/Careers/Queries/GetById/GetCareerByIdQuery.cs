using GiveAID.Application.Features.Careers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Careers.Queries.GetById;

/// <summary>
/// Query to get a career by ID.
/// </summary>
public class GetCareerByIdQuery : IRequest<CareerDto>
{
    public int CareerId { get; set; }
}

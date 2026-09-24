using GiveAID.Application.Features.Organizations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Organizations.Queries.GetAll;

/// <summary>
/// Query to get all organizations.
/// </summary>
public class GetAllOrganizationsQuery : IRequest<IEnumerable<OrganizationDto>>
{
    public string? Type { get; set; }
    public bool ActiveOnly { get; set; } = true;
}

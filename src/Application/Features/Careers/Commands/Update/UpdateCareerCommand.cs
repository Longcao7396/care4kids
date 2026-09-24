using GiveAID.Application.Features.Careers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Update;

/// <summary>
/// Command to update a career listing.
/// </summary>
public class UpdateCareerCommand : IRequest<CareerDto>
{
    public int CareerId { get; set; }
    public string? PositionTitle { get; set; }
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? SalaryRange { get; set; }
    public int? Vacancies { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool? IsActive { get; set; }
}

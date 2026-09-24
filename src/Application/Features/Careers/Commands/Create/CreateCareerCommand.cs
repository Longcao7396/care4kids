using GiveAID.Application.Features.Careers.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Create;

/// <summary>
/// Command to create a career listing.
/// </summary>
public class CreateCareerCommand : IRequest<CareerDto>
{
    public string PositionTitle { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? SalaryRange { get; set; }
    public int Vacancies { get; set; } = 1;
    public DateTime? ClosingDate { get; set; }
    public int? CreatedBy { get; set; }
}

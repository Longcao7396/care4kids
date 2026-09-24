namespace GiveAID.Application.Features.Careers.DTOs;

/// <summary>
/// DTO for career data.
/// </summary>
public class CareerDto
{
    public int CareerId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? SalaryRange { get; set; }
    public int Vacancies { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool IsActive { get; set; }
}

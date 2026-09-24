namespace GiveAID.Application.Features.Causes.DTOs;

/// <summary>
/// DTO for cause data.
/// </summary>
public class CauseDto
{
    public int CauseId { get; set; }
    public string? CauseCode { get; set; }
    public string CauseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal RaisedAmount { get; set; }
    public decimal PercentageReached { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int? ParentCauseId { get; set; }
    public bool IsParentCause { get; set; }
    public List<CauseDto>? SubCauses { get; set; }
}

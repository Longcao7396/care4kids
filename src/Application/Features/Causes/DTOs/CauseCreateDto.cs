namespace GiveAID.Application.Features.Causes.DTOs;

/// <summary>
/// DTO for creating a new cause.
/// </summary>
public class CauseCreateDto
{
    public string CauseCode { get; set; } = string.Empty;
    public string CauseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal TargetAmount { get; set; }
    public int? ParentCauseId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for updating an existing cause.
/// </summary>
public class CauseUpdateDto
{
    public string? CauseCode { get; set; }
    public string? CauseName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal? TargetAmount { get; set; }
    public int? ParentCauseId { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }
}

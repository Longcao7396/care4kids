namespace GiveAID.Application.Features.Faqs.DTOs;

/// <summary>
/// DTO for FAQ data.
/// </summary>
public class FaqDto
{
    public int FaqId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
}

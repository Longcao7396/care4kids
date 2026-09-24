namespace GiveAID.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO for recent donation data.
/// </summary>
public class RecentDonationDto
{
    public int DonationId { get; set; }
    public decimal Amount { get; set; }
    public string? DonorName { get; set; }
    public string? CampaignName { get; set; }
    public string? CauseName { get; set; }
    public DateTime DonationDate { get; set; }
    public bool IsAnonymous { get; set; }
}

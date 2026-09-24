namespace GiveAID.Application.Features.Donations.DTOs;

/// <summary>
/// DTO for donation summary (lighter weight for lists).
/// </summary>
public class DonationSummaryDto
{
    public int DonationId { get; set; }
    public decimal Amount { get; set; }
    public string? CauseName { get; set; }
    public string? CampaignName { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime DonationDate { get; set; }
    public bool IsAnonymous { get; set; }
}

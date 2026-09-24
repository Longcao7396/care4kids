namespace GiveAID.Application.Features.Donations.DTOs;

/// <summary>
/// DTO for donation data.
/// </summary>
public class DonationDto
{
    public int DonationId { get; set; }
    // UserId is nullable to support anonymous donations
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int CauseId { get; set; }
    public string? CauseName { get; set; }
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? CardLastFour { get; set; }
    public string? CardType { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public bool ReceiptSent { get; set; }
    public DateTime DonationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PaymentGateway { get; set; }
    public string? ClientSecret { get; set; }
}

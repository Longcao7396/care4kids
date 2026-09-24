namespace GiveAID.Application.Features.Donations.DTOs;

/// <summary>
/// DTO for creating a new donation.
/// </summary>
public class CreateDonationDto
{
    public int? UserId { get; set; }
    public int CauseId { get; set; }
    public int? CampaignId { get; set; }
    public int? OrganizationId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "stripe";
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Email { get; set; }
}

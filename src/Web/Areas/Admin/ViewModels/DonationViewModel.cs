namespace GiveAID.Web.Areas.Admin.ViewModels;

public class DonationViewModel
{
    public int DonationId { get; set; }
    public decimal Amount { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string DonorEmail { get; set; } = string.Empty;
    public string? CampaignName { get; set; }
    public int? CampaignId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

public class DonationListItem
{
    public int DonationId { get; set; }
    public decimal Amount { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? CampaignName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public DateTime CreatedAt { get; set; }
}

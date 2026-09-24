namespace GiveAID.Web.Areas.Admin.ViewModels;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalCauses { get; set; }
    public int TotalCampaigns { get; set; }
    public int TotalDonations { get; set; }
    public decimal TotalAmount { get; set; }
    public List<DonationChartItem> DonationsChart { get; set; } = new();
    public List<DonationRow> RecentDonations { get; set; } = new();
}

public class DonationChartItem
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class DonationRow
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

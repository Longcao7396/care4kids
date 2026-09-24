namespace GiveAID.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO for dashboard statistics.
/// </summary>
public class DashboardStatsDto
{
    public decimal TotalDonations { get; set; }
    public int TotalDonors { get; set; }
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int TotalCauses { get; set; }
    public int RegisteredUsers { get; set; }
    public decimal TotalRaisedThisMonth { get; set; }
    public decimal TotalRaisedToday { get; set; }
    public int DonationsToday { get; set; }
    public int DonationsThisMonth { get; set; }
    public List<RecentDonationDto> RecentDonations { get; set; } = new();
}

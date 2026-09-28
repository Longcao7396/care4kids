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

    /// <summary>
    /// Bug #2 fix: monthly donation totals for the trend chart (oldest to newest).
    /// </summary>
    public List<MonthlyDonationDto> DonationsByMonth { get; set; } = new();
}

/// <summary>
/// Aggregated donation total/count for a single calendar month.
/// </summary>
public class MonthlyDonationDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
    public int Count { get; set; }
}

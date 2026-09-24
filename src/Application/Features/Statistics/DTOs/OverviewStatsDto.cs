namespace GiveAID.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO for overview statistics.
/// </summary>
public class OverviewStatsDto
{
    public decimal TotalRaised { get; set; }
    public int TotalDonors { get; set; }
    public int TotalCampaigns { get; set; }
    public int ActiveCampaigns { get; set; }
    public int TotalBeneficiaries { get; set; }
    public decimal AverageDonation { get; set; }
}

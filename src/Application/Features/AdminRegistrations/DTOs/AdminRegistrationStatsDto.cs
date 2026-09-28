namespace GiveAID.Application.Features.AdminRegistrations.DTOs;

/// <summary>
/// Counts of registrations grouped by status and type — useful for admin dashboard chips.
/// </summary>
public class AdminRegistrationStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int CampaignCount { get; set; }
    public int ProgrammeCount { get; set; }
}
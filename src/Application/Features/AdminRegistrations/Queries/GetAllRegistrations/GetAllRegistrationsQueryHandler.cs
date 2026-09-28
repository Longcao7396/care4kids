using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.AdminRegistrations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.AdminRegistrations.Queries.GetAllRegistrations;

/// <summary>
/// Loads every campaign + programme registration, optionally filtered,
/// and projects them into a unified <see cref="AdminRegistrationDto"/>.
/// </summary>
public class GetAllRegistrationsQueryHandler
    : IRequestHandler<GetAllRegistrationsQuery, AdminRegistrationPageResult>
{
    private readonly IApplicationDbContext _context;

    public GetAllRegistrationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRegistrationPageResult> Handle(
        GetAllRegistrationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 25 : request.PageSize, 1, 200);

        var type = (request.Type ?? string.Empty).Trim();
        var status = (request.Status ?? string.Empty).Trim();
        var search = (request.Search ?? string.Empty).Trim().ToLower();

        var includeCampaign = string.IsNullOrEmpty(type) ||
                              string.Equals(type, "Campaign", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(type, "All", StringComparison.OrdinalIgnoreCase);

        var includeProgramme = string.IsNullOrEmpty(type) ||
                               string.Equals(type, "Programme", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(type, "All", StringComparison.OrdinalIgnoreCase);

        var combined = new List<AdminRegistrationDto>();

        if (includeCampaign)
        {
            var campaignQuery = _context.CampaignRegistrations
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                campaignQuery = campaignQuery.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                campaignQuery = campaignQuery.Where(r =>
                    (r.User != null && (
                        (r.User.FullName ?? string.Empty).ToLower().Contains(search) ||
                        (r.User.Email ?? string.Empty).ToLower().Contains(search)
                    )) ||
                    (r.Campaign != null && (r.Campaign.CampaignName ?? string.Empty).ToLower().Contains(search)));
            }

            var campaignItems = await campaignQuery
                .OrderByDescending(r => r.RegistrationDate)
                .Select(r => new AdminRegistrationDto
                {
                    RegistrationId = r.RegistrationId,
                    RegistrationType = "Campaign",
                    CampaignId = r.CampaignId,
                    CampaignName = r.Campaign != null ? r.Campaign.CampaignName : null,
                    ProgrammeId = 0,
                    ProgrammeName = null,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.FullName : null,
                    UserEmail = r.User != null ? r.User.Email : null,
                    Status = r.Status,
                    Notes = r.Notes,
                    AttendanceConfirmed = r.AttendanceConfirmed,
                    RegistrationDate = r.RegistrationDate,
                    ReviewedAt = r.UpdatedAt,
                    ReviewedBy = r.UpdatedBy,
                    RejectionReason = null,
                })
                .ToListAsync(cancellationToken);

            combined.AddRange(campaignItems);
        }

        if (includeProgramme)
        {
            var programmeQuery = _context.ProgrammeRegistrations
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                programmeQuery = programmeQuery.Where(r => r.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                programmeQuery = programmeQuery.Where(r =>
                    (r.User != null && (
                        (r.User.FullName ?? string.Empty).ToLower().Contains(search) ||
                        (r.User.Email ?? string.Empty).ToLower().Contains(search)
                    )) ||
                    (r.Programme != null && (r.Programme.Title ?? string.Empty).ToLower().Contains(search)));
            }

            var programmeItems = await programmeQuery
                .OrderByDescending(r => r.RegistrationDate)
                .Select(r => new AdminRegistrationDto
                {
                    RegistrationId = r.RegistrationId,
                    RegistrationType = "Programme",
                    CampaignId = 0,
                    CampaignName = null,
                    ProgrammeId = r.ProgrammeId,
                    ProgrammeName = r.Programme != null ? r.Programme.Title : null,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.FullName : null,
                    UserEmail = r.User != null ? r.User.Email : null,
                    Status = r.Status,
                    Notes = r.Notes,
                    AttendanceConfirmed = r.AttendanceConfirmed,
                    RegistrationDate = r.RegistrationDate,
                    ReviewedAt = r.UpdatedAt,
                    ReviewedBy = r.UpdatedBy,
                    RejectionReason = null,
                })
                .ToListAsync(cancellationToken);

            combined.AddRange(programmeItems);
        }

        // Mixed sort then page in memory. For very large datasets this could be
        // pushed to SQL with a UNION ALL + ROW_NUMBER, but admin review volumes
        // are small enough that the in-memory path is fine and keeps the code
        // straightforward.
        var ordered = combined
            .OrderByDescending(r => r.RegistrationDate)
            .ToList();

        var total = ordered.Count;
        var paged = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new AdminRegistrationPageResult
        {
            Items = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
        };
    }
}
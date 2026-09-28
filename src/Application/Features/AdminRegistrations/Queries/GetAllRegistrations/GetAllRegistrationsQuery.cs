using GiveAID.Application.Features.AdminRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.AdminRegistrations.Queries.GetAllRegistrations;

/// <summary>
/// Admin query: list every registration (campaign + programme) with filters and pagination.
/// </summary>
public class GetAllRegistrationsQuery : IRequest<AdminRegistrationPageResult>
{
    /// <summary>Optional status filter: "Pending", "Approved", "Rejected", "Cancelled", or "Registered".</summary>
    public string? Status { get; set; }

    /// <summary>Optional registration type filter: "Campaign" or "Programme".</summary>
    public string? Type { get; set; }

    /// <summary>Optional free-text search across user name/email and the parent entity name.</summary>
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Paged result envelope for <see cref="GetAllRegistrationsQuery"/>.
/// </summary>
public class AdminRegistrationPageResult
{
    public IEnumerable<AdminRegistrationDto> Items { get; set; } = Array.Empty<AdminRegistrationDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
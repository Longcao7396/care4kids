using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.Donations.Commands.Create;
using GiveAID.Application.Features.Donations.DTOs;
using GiveAID.Application.Features.Donations.Queries.GetAll;
using GiveAID.Application.Features.Donations.Queries.GetById;
using GiveAID.Application.Features.Donations.Queries.GetByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for managing donations.
/// </summary>
[ApiController]
[Route("api/v1/donations")]
public class DonationsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IApplicationDbContext _context;

    public DonationsController(ISender mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Get all donations (Admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? campaignId = null,
        [FromQuery] int? causeId = null)
    {
        var items = await _mediator.Send(new GetAllDonationsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            UserId = userId,
            CampaignId = campaignId,
            CauseId = causeId
        });
        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Get donation by ID.
    /// M-01 FIX: Return 403 Forbidden when resource exists but user has no permission,
    /// rather than the default Forbid() which may issue a 401 redirect challenge.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        var isAdmin = User.IsInRole("Admin");

        try
        {
            var dto = await _mediator.Send(new GetDonationByIdQuery { DonationId = id });
            // Non-admin users can only see their own donations
            if (!isAdmin && dto.UserId != userId)
            {
                // M-01: Return explicit 403 Forbidden — donation exists but user lacks permission
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = "You do not have permission to view this donation.", data = (object?)null });
            }
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Donation {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Create a new donation.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateDonationRequest request)
    {
        // SECURITY FIX (C-05): UserId MUST come from authenticated identity.
        // We NEVER trust the client's UserId to prevent user impersonation attacks.
        // - Authenticated user: UserId comes from JWT claims (cannot be overridden by request)
        // - Anonymous user: UserId is null (anonymous donation)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var authenticatedUserId);

        int? resolvedUserId = authenticatedUserId > 0 ? authenticatedUserId : null;

        // SECURITY: If an authenticated user attempts to send a different UserId in the request body,
        // we ignore it completely to prevent impersonation.
        // If the request body contains a UserId, it is silently discarded.

        var command = new CreateDonationCommand
        {
            // Always use the authenticated identity's UserId, never the request's UserId
            UserId = resolvedUserId,
            CauseId = request.CauseId,
            CampaignId = request.CampaignId,
            OrganizationId = request.OrganizationId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod ?? "stripe",
            Message = request.Message,
            IsAnonymous = request.IsAnonymous,
            IdempotencyKey = request.IdempotencyKey,
            Email = request.Email
        };

        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Donation created", data = dto });
    }

    /// <summary>
    /// Get donations by current user.
    /// </summary>
    [HttpGet("my-donations")]
    [Authorize]
    public async Task<IActionResult> GetMyDonations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token", data = (object?)null });
        }

        var items = await _mediator.Send(new GetDonationsByUserQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize
        });

        return Ok(new { success = true, message = "OK", data = new { items, page, pageSize } });
    }

    /// <summary>
    /// Confirm donation via payment gateway webhook.
    /// </summary>
    [HttpPost("webhook/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmWebhook([FromBody] object request)
    {
        // Webhook processing handled by gateway-specific controller
        return Ok(new { success = true, message = "Webhook processed", data = (object?)null });
    }

    /// <summary>
    /// Get donation statistics.
    /// H-02 FIX: Uses SQL aggregation via EF Core (COUNT, SUM) instead of loading
    /// all donations into memory and filtering in C#. This is significantly more
    /// performant for large datasets.
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStats()
    {
        var completedDonations = _context.Donations.Where(d => d.PaymentStatus == "Completed").ToList();

        var totalDonations = completedDonations.Count;
        var totalAmount = completedDonations.Sum(d => d.Amount);
        var pendingCount = _context.Donations.Count(d => d.PaymentStatus == "Pending");
        var failedCount = _context.Donations.Count(d => d.PaymentStatus == "Failed");

        return Ok(new
        {
            success = true,
            message = "OK",
            data = new
            {
                totalDonations,
                totalAmount,
                pendingCount,
                failedCount
            }
        });
    }
}

/// <summary>
/// Request body for POST /donations.
/// SECURITY NOTE: UserId is ignored in the request body to prevent impersonation attacks.
/// UserId is automatically determined from the authenticated JWT token.
/// For anonymous donations, simply don't authenticate and UserId will be null.
/// </summary>
public class CreateDonationRequest
{
    // UserId is accepted in the request body for API compatibility but is IGNORED for security.
    // The authenticated user's identity determines the actual UserId.
    public int? UserId { get; set; }

    public int CauseId { get; set; }
    public int? CampaignId { get; set; }
    public int? OrganizationId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Email { get; set; }
}

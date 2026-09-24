using GiveAID.Application.Features.Achievements.Commands.Create;
using GiveAID.Application.Features.Achievements.Commands.Delete;
using GiveAID.Application.Features.Achievements.Commands.Update;
using GiveAID.Application.Features.Achievements.DTOs;
using GiveAID.Application.Features.Achievements.Queries.GetAll;
using GiveAID.Application.Features.Achievements.Queries.GetFeatured;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for achievements/milestones.
/// </summary>
[ApiController]
[Route("api/v1/achievements")]
public class AchievementsController : ControllerBase
{
    private readonly ISender _mediator;

    public AchievementsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all achievements.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var items = await _mediator.Send(new GetAllAchievementsQuery { ActiveOnly = activeOnly });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get featured achievements.
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeatured([FromQuery] int limit = 6)
    {
        var items = await _mediator.Send(new GetFeaturedAchievementsQuery { Limit = limit });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get achievement statistics (MAJ-007).
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStats()
    {
        var all = await _mediator.Send(new GetAllAchievementsQuery { ActiveOnly = false });
        var allList = all.ToList();
        var featured = allList.Count(a => a.IsFeatured);
        return Ok(new
        {
            success = true,
            message = "OK",
            data = new
            {
                totalAchievements = allList.Count,
                featuredAchievements = featured,
                totalBeneficiaries = allList.Sum(a => a.Beneficiaries ?? 0)
            }
        });
    }

    /// <summary>
    /// Get achievement by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var all = await _mediator.Send(new GetAllAchievementsQuery { ActiveOnly = false });
        var dto = all.FirstOrDefault(a => a.AchievementId == id);
        if (dto == null)
        {
            return NotFound(new { success = false, message = $"Achievement {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "OK", data = dto });
    }

    /// <summary>
    /// Create a new achievement (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAchievementRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var command = new CreateAchievementCommand
        {
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            MetricValue = request.MetricValue,
            MetricLabel = request.MetricLabel,
            MetricSuffix = request.MetricSuffix,
            AchievementDate = request.AchievementDate,
            ImageUrl = request.ImageUrl,
            Icon = request.Icon,
            AwardBy = request.AwardBy,
            Location = request.Location,
            Beneficiaries = request.Beneficiaries,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            CreatedBy = userId > 0 ? userId : null
        };

        var dto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = dto.AchievementId },
            new { success = true, message = "Achievement created", data = dto });
    }

    /// <summary>
    /// Update an achievement (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAchievementRequest request)
    {
        var command = new UpdateAchievementCommand
        {
            AchievementId = id,
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            MetricValue = request.MetricValue,
            MetricLabel = request.MetricLabel,
            MetricSuffix = request.MetricSuffix,
            AchievementDate = request.AchievementDate,
            ImageUrl = request.ImageUrl,
            Icon = request.Icon,
            AwardBy = request.AwardBy,
            Location = request.Location,
            Beneficiaries = request.Beneficiaries,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder
        };

        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Achievement updated", data = dto });
    }

    /// <summary>
    /// Delete an achievement (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteAchievementCommand { AchievementId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Achievement {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Achievement deleted", data = (object?)null });
    }
}

/// <summary>
/// Request body for POST /achievements.
/// </summary>
public class CreateAchievementRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal? MetricValue { get; set; }
    public string? MetricLabel { get; set; }
    public string? MetricSuffix { get; set; }
    public DateTime? AchievementDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }
    public string? AwardBy { get; set; }
    public string? Location { get; set; }
    public int? Beneficiaries { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Request body for PUT /achievements/{id}.
/// </summary>
public class UpdateAchievementRequest
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal? MetricValue { get; set; }
    public string? MetricLabel { get; set; }
    public string? MetricSuffix { get; set; }
    public DateTime? AchievementDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? Icon { get; set; }
    public string? AwardBy { get; set; }
    public string? Location { get; set; }
    public int? Beneficiaries { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
}

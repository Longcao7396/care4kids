using GiveAID.Application.Features.CampaignReports.Commands.Create;
using GiveAID.Application.Features.CampaignReports.Commands.Delete;
using GiveAID.Application.Features.CampaignReports.Commands.Update;
using GiveAID.Application.Features.CampaignReports.Queries.GetAll;
using GiveAID.Application.Features.CampaignReports.Queries.GetByCampaign;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for campaign reports.
/// </summary>
[ApiController]
[Route("api/v1/campaign-reports")]
public class CampaignReportsController : ControllerBase
{
    private readonly ISender _mediator;

    public CampaignReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all campaign reports.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool publishedOnly = false)
    {
        var items = await _mediator.Send(new GetAllCampaignReportsQuery { PublishedOnly = publishedOnly });
        var list = items.ToList();
        return Ok(new
        {
            success = true,
            message = "OK",
            data = new
            {
                items = list.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                totalCount = list.Count
            }
        });
    }

    /// <summary>
    /// Get reports for a specific campaign.
    /// </summary>
    [HttpGet("campaign/{campaignId:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCampaign(int campaignId)
    {
        var items = await _mediator.Send(new GetCampaignReportsByCampaignQuery { CampaignId = campaignId });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Create a new campaign report (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignReportRequest request)
    {
        var dto = await _mediator.Send(new CreateCampaignReportCommand
        {
            CampaignId = request.CampaignId,
            TotalReceived = request.TotalReceived,
            TotalSpent = request.TotalSpent,
            BeneficiariesReached = request.BeneficiariesReached,
            ReportTitle = request.ReportTitle,
            ReportContent = request.ReportContent,
            ExpenseBreakdown = request.ExpenseBreakdown,
            Photos = request.Photos,
            Documents = request.Documents
        });
        return Ok(new { success = true, message = "Report created", data = dto });
    }

    /// <summary>
    /// Update a campaign report (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignReportRequest request)
    {
        var dto = await _mediator.Send(new UpdateCampaignReportCommand
        {
            ReportId = id,
            TotalReceived = request.TotalReceived,
            TotalSpent = request.TotalSpent,
            BeneficiariesReached = request.BeneficiariesReached,
            ReportTitle = request.ReportTitle,
            ReportContent = request.ReportContent,
            ExpenseBreakdown = request.ExpenseBreakdown,
            Photos = request.Photos,
            Documents = request.Documents
        });
        return Ok(new { success = true, message = "Report updated", data = dto });
    }

    /// <summary>
    /// Delete a campaign report (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteCampaignReportCommand { ReportId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Report {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Report deleted", data = new { reportId = id } });
    }
}

/// <summary>
/// Request body for POST /campaign-reports.
/// </summary>
public class CreateCampaignReportRequest
{
    public int CampaignId { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalSpent { get; set; }
    public int? BeneficiariesReached { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
}

/// <summary>
/// Request body for PUT /campaign-reports/{id}.
/// </summary>
public class UpdateCampaignReportRequest
{
    public decimal? TotalReceived { get; set; }
    public decimal? TotalSpent { get; set; }
    public int? BeneficiariesReached { get; set; }
    public string? ReportTitle { get; set; }
    public string? ReportContent { get; set; }
    public string? ExpenseBreakdown { get; set; }
    public string? Photos { get; set; }
    public string? Documents { get; set; }
}

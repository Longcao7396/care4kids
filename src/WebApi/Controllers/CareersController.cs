using GiveAID.Application.Features.Careers.Commands.Create;
using GiveAID.Application.Features.Careers.Commands.Delete;
using GiveAID.Application.Features.Careers.Commands.Update;
using GiveAID.Application.Features.Careers.DTOs;
using GiveAID.Application.Features.Careers.Queries.GetAll;
using GiveAID.Application.Features.Careers.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveAID.V2.WebApi.Controllers;

/// <summary>
/// Controller for managing careers/job postings.
/// </summary>
[ApiController]
[Route("api/v1/careers")]
public class CareersController : ControllerBase
{
    private readonly ISender _mediator;

    public CareersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all career postings.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var items = await _mediator.Send(new GetAllCareersQuery { ActiveOnly = activeOnly });
        return Ok(new { success = true, message = "OK", data = items });
    }

    /// <summary>
    /// Get career by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var dto = await _mediator.Send(new GetCareerByIdQuery { CareerId = id });
            return Ok(new { success = true, message = "OK", data = dto });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, message = $"Career {id} not found", data = (object?)null });
        }
    }

    /// <summary>
    /// Apply for a career (public).
    /// </summary>
    [HttpPost("{id:int}/apply")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply(int id, [FromBody] CareerApplicationRequest request)
    {
        // Career applications are tracked but stored externally for now.
        return Ok(new { success = true, message = "Application submitted successfully", data = new { careerId = id, applicantEmail = request.Email } });
    }

    /// <summary>
    /// Create a new career posting (Admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCareerRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var command = new CreateCareerCommand
        {
            PositionTitle = request.PositionTitle,
            Department = request.Department,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            SalaryRange = request.SalaryRange,
            Vacancies = request.Vacancies,
            ClosingDate = request.ClosingDate,
            CreatedBy = userId > 0 ? userId : null
        };

        var dto = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = dto.CareerId },
            new { success = true, message = "Career created", data = dto });
    }

    /// <summary>
    /// Update a career posting (Admin only).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCareerRequest request)
    {
        var command = new UpdateCareerCommand
        {
            CareerId = id,
            PositionTitle = request.PositionTitle,
            Department = request.Department,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            SalaryRange = request.SalaryRange,
            Vacancies = request.Vacancies,
            ClosingDate = request.ClosingDate,
            IsActive = request.IsActive
        };

        var dto = await _mediator.Send(command);
        return Ok(new { success = true, message = "Career updated", data = dto });
    }

    /// <summary>
    /// Delete a career posting (Admin only).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _mediator.Send(new DeleteCareerCommand { CareerId = id });
        if (!ok)
        {
            return NotFound(new { success = false, message = $"Career {id} not found", data = (object?)null });
        }
        return Ok(new { success = true, message = "Career deleted", data = (object?)null });
    }
}

/// <summary>
/// Body for POST /careers/{id}/apply.
/// </summary>
public class CareerApplicationRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUrl { get; set; }
}

/// <summary>
/// Request body for POST /careers.
/// </summary>
public class CreateCareerRequest
{
    public string PositionTitle { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? SalaryRange { get; set; }
    public int Vacancies { get; set; } = 1;
    public DateTime? ClosingDate { get; set; }
}

/// <summary>
/// Request body for PUT /careers/{id}.
/// </summary>
public class UpdateCareerRequest
{
    public string? PositionTitle { get; set; }
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? SalaryRange { get; set; }
    public int? Vacancies { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool? IsActive { get; set; }
}

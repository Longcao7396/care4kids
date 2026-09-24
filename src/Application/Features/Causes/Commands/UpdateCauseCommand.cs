using GiveAID.Application.Features.Causes.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Causes.Commands;

/// <summary>
/// Command to update an existing cause.
/// </summary>
public class UpdateCauseCommand : IRequest
{
    public int CauseId { get; }
    public string? CauseCode { get; set; }
    public string? CauseName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal? TargetAmount { get; set; }
    public int? ParentCauseId { get; set; }
    public bool? IsActive { get; set; }
    public int? DisplayOrder { get; set; }

    public UpdateCauseCommand(int causeId, CauseUpdateDto dto)
    {
        CauseId = causeId;
        CauseCode = dto.CauseCode;
        CauseName = dto.CauseName;
        Description = dto.Description;
        Icon = dto.Icon;
        TargetAmount = dto.TargetAmount;
        ParentCauseId = dto.ParentCauseId;
        IsActive = dto.IsActive;
        DisplayOrder = dto.DisplayOrder;
    }
}

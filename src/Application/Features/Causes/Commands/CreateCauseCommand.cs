using GiveAID.Application.Features.Causes.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Causes.Commands;

/// <summary>
/// Command to create a new cause.
/// </summary>
public class CreateCauseCommand : IRequest<int>
{
    public string CauseCode { get; set; } = string.Empty;
    public string CauseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal TargetAmount { get; set; }
    public int? ParentCauseId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public CreateCauseCommand() { }

    public CreateCauseCommand(CauseCreateDto dto)
    {
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

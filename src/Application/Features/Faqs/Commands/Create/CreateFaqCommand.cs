using GiveAID.Application.Features.Faqs.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Create;

/// <summary>
/// Command to create a FAQ.
/// </summary>
public class CreateFaqCommand : IRequest<FaqDto>
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public int? CreatedBy { get; set; }
}

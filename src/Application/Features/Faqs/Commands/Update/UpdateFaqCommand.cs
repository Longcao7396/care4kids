using GiveAID.Application.Features.Faqs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Update;

/// <summary>
/// Command to update a FAQ.
/// </summary>
public class UpdateFaqCommand : IRequest<FaqDto>
{
    public int FaqId { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
}

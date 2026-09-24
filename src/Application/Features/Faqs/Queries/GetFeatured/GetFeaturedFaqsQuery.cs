using GiveAID.Application.Features.Faqs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Queries.GetFeatured;

/// <summary>
/// Query to get featured FAQs.
/// </summary>
public class GetFeaturedFaqsQuery : IRequest<IEnumerable<FaqDto>>
{
    public int Limit { get; set; } = 10;
}

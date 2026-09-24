using GiveAID.Application.Features.Faqs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Queries.GetAll;

/// <summary>
/// Query to get all FAQs.
/// </summary>
public class GetAllFaqsQuery : IRequest<IEnumerable<FaqDto>>
{
    public bool ActiveOnly { get; set; } = true;
}

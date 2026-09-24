using GiveAID.Application.Features.Faqs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Queries.GetCategories;

/// <summary>
/// Query to get FAQ categories.
/// </summary>
public class GetFaqCategoriesQuery : IRequest<IEnumerable<string>>
{
}

using GiveAID.Application.Features.Faqs.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Faqs.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedFaqsQuery.
/// </summary>
public class GetFeaturedFaqsQueryHandler : IRequestHandler<GetFeaturedFaqsQuery, IEnumerable<FaqDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedFaqsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FaqDto>> Handle(GetFeaturedFaqsQuery request, CancellationToken cancellationToken)
    {
        var faqs = await _context.Faqs
            .Where(f => f.IsFeatured && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return faqs.Select(f => new FaqDto
        {
            FaqId = f.FaqId,
            Question = f.Question,
            Answer = f.Answer,
            Category = f.Category,
            DisplayOrder = f.DisplayOrder,
            IsActive = f.IsActive,
            IsFeatured = f.IsFeatured,
            ViewCount = f.ViewCount
        });
    }
}

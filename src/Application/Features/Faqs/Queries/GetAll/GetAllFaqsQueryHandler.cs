using GiveAID.Application.Features.Faqs.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Faqs.Queries.GetAll;

/// <summary>
/// Handler for GetAllFaqsQuery.
/// </summary>
public class GetAllFaqsQueryHandler : IRequestHandler<GetAllFaqsQuery, IEnumerable<FaqDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllFaqsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FaqDto>> Handle(GetAllFaqsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Faqs.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(f => f.IsActive);
        }

        var faqs = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Question)
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

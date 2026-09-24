using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Faqs.Queries.GetCategories;

/// <summary>
/// Handler for GetFaqCategoriesQuery.
/// </summary>
public class GetFaqCategoriesQueryHandler : IRequestHandler<GetFaqCategoriesQuery, IEnumerable<string>>
{
    private readonly IApplicationDbContext _context;

    public GetFaqCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> Handle(GetFaqCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Faqs
            .Where(f => f.IsActive && f.Category != null)
            .Select(f => f.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        return categories;
    }
}

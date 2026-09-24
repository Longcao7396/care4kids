using GiveAID.Application.Features.Careers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Careers.Queries.GetAll;

/// <summary>
/// Handler for GetAllCareersQuery.
/// </summary>
public class GetAllCareersQueryHandler : IRequestHandler<GetAllCareersQuery, IEnumerable<CareerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCareersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CareerDto>> Handle(GetAllCareersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Careers.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(c => c.IsActive && (c.ClosingDate == null || c.ClosingDate > DateTime.UtcNow));
        }

        var careers = await query
            .OrderByDescending(c => c.PostedDate)
            .ToListAsync(cancellationToken);

        return careers.Select(c => new CareerDto
        {
            CareerId = c.CareerId,
            PositionTitle = c.PositionTitle,
            Department = c.Department,
            Description = c.Description,
            Requirements = c.Requirements,
            Responsibilities = c.Responsibilities,
            Location = c.Location,
            EmploymentType = c.EmploymentType,
            SalaryRange = c.SalaryRange,
            Vacancies = c.Vacancies,
            PostedDate = c.PostedDate,
            ClosingDate = c.ClosingDate,
            IsActive = c.IsActive
        });
    }
}

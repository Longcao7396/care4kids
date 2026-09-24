using GiveAID.Application.Features.Careers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Careers.Queries.GetActive;

/// <summary>
/// Handler for GetActiveCareersQuery.
/// </summary>
public class GetActiveCareersQueryHandler : IRequestHandler<GetActiveCareersQuery, IEnumerable<CareerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveCareersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CareerDto>> Handle(GetActiveCareersQuery request, CancellationToken cancellationToken)
    {
        var careers = await _context.Careers
            .Where(c => c.IsActive && (c.ClosingDate == null || c.ClosingDate > DateTime.UtcNow))
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

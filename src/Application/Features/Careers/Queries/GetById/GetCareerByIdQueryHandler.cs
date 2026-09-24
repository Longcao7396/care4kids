using GiveAID.Application.Features.Careers.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Careers.Queries.GetById;

/// <summary>
/// Handler for GetCareerByIdQuery.
/// </summary>
public class GetCareerByIdQueryHandler : IRequestHandler<GetCareerByIdQuery, CareerDto>
{
    private readonly IApplicationDbContext _context;

    public GetCareerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CareerDto> Handle(GetCareerByIdQuery request, CancellationToken cancellationToken)
    {
        var career = await _context.Careers.FindAsync(new object[] { request.CareerId }, cancellationToken);

        if (career == null)
        {
            throw new InvalidOperationException($"Career with ID {request.CareerId} not found.");
        }

        return new CareerDto
        {
            CareerId = career.CareerId,
            PositionTitle = career.PositionTitle,
            Department = career.Department,
            Description = career.Description,
            Requirements = career.Requirements,
            Responsibilities = career.Responsibilities,
            Location = career.Location,
            EmploymentType = career.EmploymentType,
            SalaryRange = career.SalaryRange,
            Vacancies = career.Vacancies,
            PostedDate = career.PostedDate,
            ClosingDate = career.ClosingDate,
            IsActive = career.IsActive
        };
    }
}

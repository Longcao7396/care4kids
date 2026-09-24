using GiveAID.Application.Features.Careers.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Create;

/// <summary>
/// Handler for CreateCareerCommand.
/// </summary>
public class CreateCareerCommandHandler : IRequestHandler<CreateCareerCommand, CareerDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCareerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CareerDto> Handle(CreateCareerCommand request, CancellationToken cancellationToken)
    {
        var career = new Career
        {
            PositionTitle = request.PositionTitle,
            Department = request.Department,
            Description = request.Description,
            Requirements = request.Requirements,
            Responsibilities = request.Responsibilities,
            Location = request.Location,
            EmploymentType = request.EmploymentType,
            SalaryRange = request.SalaryRange,
            Vacancies = request.Vacancies,
            PostedDate = DateTime.UtcNow,
            ClosingDate = request.ClosingDate,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Careers.Add(career);
        await _context.SaveChangesAsync(cancellationToken);

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

using GiveAID.Application.Features.Careers.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Update;

/// <summary>
/// Handler for UpdateCareerCommand.
/// </summary>
public class UpdateCareerCommandHandler : IRequestHandler<UpdateCareerCommand, CareerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCareerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CareerDto> Handle(UpdateCareerCommand request, CancellationToken cancellationToken)
    {
        var career = await _context.Careers.FindAsync(new object[] { request.CareerId }, cancellationToken);

        if (career == null)
        {
            throw new InvalidOperationException($"Career with ID {request.CareerId} not found.");
        }

        if (request.PositionTitle != null) career.PositionTitle = request.PositionTitle;
        if (request.Department != null) career.Department = request.Department;
        if (request.Description != null) career.Description = request.Description;
        if (request.Requirements != null) career.Requirements = request.Requirements;
        if (request.Responsibilities != null) career.Responsibilities = request.Responsibilities;
        if (request.Location != null) career.Location = request.Location;
        if (request.EmploymentType != null) career.EmploymentType = request.EmploymentType;
        if (request.SalaryRange != null) career.SalaryRange = request.SalaryRange;
        if (request.Vacancies.HasValue) career.Vacancies = request.Vacancies.Value;
        if (request.ClosingDate.HasValue) career.ClosingDate = request.ClosingDate;
        if (request.IsActive.HasValue) career.IsActive = request.IsActive.Value;

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

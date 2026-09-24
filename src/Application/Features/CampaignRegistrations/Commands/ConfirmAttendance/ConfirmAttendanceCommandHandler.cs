using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Commands.ConfirmAttendance;

/// <summary>
/// Handler for ConfirmAttendanceCommand.
/// </summary>
public class ConfirmAttendanceCommandHandler : IRequestHandler<ConfirmAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmAttendanceCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.CampaignRegistrations.FindAsync(new object[] { request.RegistrationId }, cancellationToken);

        if (registration == null)
        {
            throw new InvalidOperationException($"Registration with ID {request.RegistrationId} not found.");
        }

        registration.AttendanceConfirmed = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

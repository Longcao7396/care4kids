using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using GiveAID.Application.Common.Interfaces;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Commands.Register;

/// <summary>
/// Handler for RegisterCampaignCommand.
/// </summary>
public class RegisterCampaignCommandHandler : IRequestHandler<RegisterCampaignCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RegisterCampaignCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<RegistrationDto> Handle(RegisterCampaignCommand request, CancellationToken cancellationToken)
    {
        // Check if already registered
        var existing = _context.CampaignRegistrations
            .FirstOrDefault(r => r.CampaignId == request.CampaignId && r.UserId == request.UserId);

        if (existing != null)
        {
            throw new InvalidOperationException("You are already registered for this campaign.");
        }

        // Check campaign capacity if limited
        var campaign = await _context.Campaigns.FindAsync(new object[] { request.CampaignId }, cancellationToken);
        if (campaign == null)
        {
            throw new InvalidOperationException("Campaign not found.");
        }

        if (campaign.MaxParticipants.HasValue)
        {
            var currentCount = _context.CampaignRegistrations.Count(r => r.CampaignId == request.CampaignId && r.Status != "Cancelled");
            if (currentCount >= campaign.MaxParticipants.Value)
            {
                throw new InvalidOperationException("This campaign has reached its maximum number of participants.");
            }
        }

        var registration = new CampaignRegistration
        {
            CampaignId = request.CampaignId,
            UserId = request.UserId,
            Status = "Registered",
            Notes = request.Notes,
            AttendanceConfirmed = false,
            RegistrationDate = DateTime.UtcNow
        };

        _context.CampaignRegistrations.Add(registration);
        await _context.SaveChangesAsync(cancellationToken);

        // Trigger notification for successful registration
        await _notificationService.CreateNotificationAsync(
            request.UserId,
            "campaign_registration",
            "Registration Confirmed",
            $"You have successfully registered for {campaign.CampaignName}. Start date: {campaign.StartDate:MMM dd, yyyy}",
            "CampaignRegistration",
            registration.RegistrationId,
            cancellationToken);

        return new RegistrationDto
        {
            RegistrationId = registration.RegistrationId,
            CampaignId = registration.CampaignId,
            CampaignName = campaign?.CampaignName,
            UserId = registration.UserId,
            Status = registration.Status,
            Notes = registration.Notes,
            AttendanceConfirmed = registration.AttendanceConfirmed,
            RegistrationDate = registration.RegistrationDate
        };
    }
}

using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.AdminRegistrations.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.AdminRegistrations.Commands.ApproveRegistration;

/// <summary>
/// Sets Status = "Approved" on the requested registration, stamps the audit
/// fields, and pushes a notification to the registrant.
/// </summary>
public class ApproveRegistrationCommandHandler
    : IRequestHandler<ApproveRegistrationCommand, AdminRegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ApproveRegistrationCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<AdminRegistrationDto> Handle(
        ApproveRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.Equals(request.RegistrationType, "Programme", StringComparison.OrdinalIgnoreCase))
        {
            return await ApproveProgrammeAsync(request, cancellationToken);
        }

        return await ApproveCampaignAsync(request, cancellationToken);
    }

    private async Task<AdminRegistrationDto> ApproveCampaignAsync(
        ApproveRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var reg = await _context.CampaignRegistrations
            .Include(r => r.Campaign)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.RegistrationId == request.RegistrationId, cancellationToken);

        if (reg == null)
        {
            throw new KeyNotFoundException($"Campaign registration {request.RegistrationId} not found.");
        }

        if (string.Equals(reg.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            // Idempotent: already approved.
            return MapCampaign(reg);
        }

        if (string.Equals(reg.Status, "Rejected", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(reg.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot approve a registration that is already {reg.Status}.");
        }

        reg.Status = "Approved";
        reg.UpdatedAt = DateTime.UtcNow;
        reg.UpdatedBy = request.ReviewedBy;

        await _context.SaveChangesAsync(cancellationToken);

        if (reg.User != null)
        {
            await _notificationService.CreateNotificationAsync(
                reg.UserId,
                "registration_approved",
                "Registration approved",
                reg.Campaign != null
                    ? $"Your registration for \"{reg.Campaign.CampaignName}\" has been approved."
                    : "Your registration has been approved.",
                "CampaignRegistration",
                reg.RegistrationId,
                cancellationToken);
        }

        return MapCampaign(reg);
    }

    private async Task<AdminRegistrationDto> ApproveProgrammeAsync(
        ApproveRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        var reg = await _context.ProgrammeRegistrations
            .Include(r => r.Programme)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.RegistrationId == request.RegistrationId, cancellationToken);

        if (reg == null)
        {
            throw new KeyNotFoundException($"Programme registration {request.RegistrationId} not found.");
        }

        if (string.Equals(reg.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return MapProgramme(reg);
        }

        if (string.Equals(reg.Status, "Rejected", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(reg.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot approve a registration that is already {reg.Status}.");
        }

        reg.Status = "Approved";
        reg.UpdatedAt = DateTime.UtcNow;
        reg.UpdatedBy = request.ReviewedBy;

        await _context.SaveChangesAsync(cancellationToken);

        if (reg.User != null)
        {
            await _notificationService.CreateNotificationAsync(
                reg.UserId,
                "registration_approved",
                "Registration approved",
                reg.Programme != null
                    ? $"Your registration for \"{reg.Programme.Title}\" has been approved."
                    : "Your registration has been approved.",
                "ProgrammeRegistration",
                reg.RegistrationId,
                cancellationToken);
        }

        return MapProgramme(reg);
    }

    private static AdminRegistrationDto MapCampaign(Domain.Entities.CampaignRegistration r) => new()
    {
        RegistrationId = r.RegistrationId,
        RegistrationType = "Campaign",
        CampaignId = r.CampaignId,
        CampaignName = r.Campaign?.CampaignName,
        ProgrammeId = 0,
        ProgrammeName = null,
        UserId = r.UserId,
        UserName = r.User?.FullName,
        UserEmail = r.User?.Email,
        Status = r.Status,
        Notes = r.Notes,
        AttendanceConfirmed = r.AttendanceConfirmed,
        RegistrationDate = r.RegistrationDate,
        ReviewedAt = r.UpdatedAt,
        ReviewedBy = r.UpdatedBy,
        RejectionReason = null,
    };

    private static AdminRegistrationDto MapProgramme(Domain.Entities.ProgrammeRegistration r) => new()
    {
        RegistrationId = r.RegistrationId,
        RegistrationType = "Programme",
        CampaignId = 0,
        CampaignName = null,
        ProgrammeId = r.ProgrammeId,
        ProgrammeName = r.Programme?.Title,
        UserId = r.UserId,
        UserName = r.User?.FullName,
        UserEmail = r.User?.Email,
        Status = r.Status,
        Notes = r.Notes,
        AttendanceConfirmed = r.AttendanceConfirmed,
        RegistrationDate = r.RegistrationDate,
        ReviewedAt = r.UpdatedAt,
        ReviewedBy = r.UpdatedBy,
        RejectionReason = null,
    };
}
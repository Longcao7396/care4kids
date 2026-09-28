using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Features.AdminRegistrations.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.AdminRegistrations.Commands.RejectRegistration;

/// <summary>
/// Sets Status = "Rejected" on the requested registration, stamps the audit
/// fields, and pushes a notification to the registrant. Stores the optional
/// rejection reason in <see cref="Domain.Entities.CampaignRegistration.Notes"/>
/// / <see cref="Domain.Entities.ProgrammeRegistration.Notes"/> since neither
/// entity has a dedicated field. The reason is prefixed with "[Rejection] "
/// so admins can distinguish it from user-supplied notes.
/// </summary>
public class RejectRegistrationCommandHandler
    : IRequestHandler<RejectRegistrationCommand, AdminRegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public RejectRegistrationCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<AdminRegistrationDto> Handle(
        RejectRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        if (string.Equals(request.RegistrationType, "Programme", StringComparison.OrdinalIgnoreCase))
        {
            return await RejectProgrammeAsync(request, cancellationToken);
        }

        return await RejectCampaignAsync(request, cancellationToken);
    }

    private async Task<AdminRegistrationDto> RejectCampaignAsync(
        RejectRegistrationCommand request,
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

        if (string.Equals(reg.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return MapCampaign(reg);
        }

        if (string.Equals(reg.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Cannot reject a registration that has already been approved.");
        }

        reg.Status = "Rejected";
        reg.UpdatedAt = DateTime.UtcNow;
        reg.UpdatedBy = request.ReviewedBy;

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            reg.Notes = string.IsNullOrWhiteSpace(reg.Notes)
                ? $"[Rejection] {request.Reason}"
                : $"{reg.Notes}\n[Rejection] {request.Reason}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (reg.User != null)
        {
            var msg = reg.Campaign != null
                ? $"Your registration for \"{reg.Campaign.CampaignName}\" was rejected."
                : "Your registration was rejected.";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                msg += $" Reason: {request.Reason}";
            }

            await _notificationService.CreateNotificationAsync(
                reg.UserId,
                "registration_rejected",
                "Registration rejected",
                msg,
                "CampaignRegistration",
                reg.RegistrationId,
                cancellationToken);
        }

        return MapCampaign(reg);
    }

    private async Task<AdminRegistrationDto> RejectProgrammeAsync(
        RejectRegistrationCommand request,
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

        if (string.Equals(reg.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return MapProgramme(reg);
        }

        if (string.Equals(reg.Status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Cannot reject a registration that has already been approved.");
        }

        reg.Status = "Rejected";
        reg.UpdatedAt = DateTime.UtcNow;
        reg.UpdatedBy = request.ReviewedBy;

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            reg.Notes = string.IsNullOrWhiteSpace(reg.Notes)
                ? $"[Rejection] {request.Reason}"
                : $"{reg.Notes}\n[Rejection] {request.Reason}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (reg.User != null)
        {
            var msg = reg.Programme != null
                ? $"Your registration for \"{reg.Programme.Title}\" was rejected."
                : "Your registration was rejected.";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                msg += $" Reason: {request.Reason}";
            }

            await _notificationService.CreateNotificationAsync(
                reg.UserId,
                "registration_rejected",
                "Registration rejected",
                msg,
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
        RejectionReason = ExtractRejectionReason(r.Notes),
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
        RejectionReason = ExtractRejectionReason(r.Notes),
    };

    private static string? ExtractRejectionReason(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;
        const string marker = "[Rejection]";
        var idx = notes.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return null;
        return notes.Substring(idx + marker.Length).Trim();
    }
}
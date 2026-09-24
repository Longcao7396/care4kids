using GiveAID.Application.Features.EmailLogs.DTOs;
using GiveAID.Application.Services;
using MediatR;

namespace GiveAID.Application.Features.EmailLogs.Commands.RetryEmail;

/// <summary>
/// Handler for RetryEmailCommand.
/// </summary>
public class RetryEmailCommandHandler : IRequestHandler<RetryEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public RetryEmailCommandHandler(IApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<bool> Handle(RetryEmailCommand request, CancellationToken cancellationToken)
    {
        var emailLog = await _context.EmailLogs.FindAsync(new object[] { request.EmailLogId }, cancellationToken);

        if (emailLog == null)
        {
            throw new InvalidOperationException($"Email log with ID {request.EmailLogId} not found.");
        }

        if (emailLog.Status == "Sent")
        {
            return true; // Already sent
        }

        var success = await _emailSender.SendEmailAsync(emailLog.ToEmail, emailLog.Subject, emailLog.Body ?? "", true);

        emailLog.UpdatedAt = DateTime.UtcNow;

        if (success)
        {
            emailLog.Status = "Sent";
            emailLog.SentAt = DateTime.UtcNow;
            emailLog.ErrorMessage = null;
        }
        else
        {
            emailLog.Status = "Failed";
            emailLog.ErrorMessage = "Retry failed";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return success;
    }
}

using GiveAID.Application.Common.Interfaces;

namespace GiveAID.Infrastructure.Email;

public class EmailLogService : IEmailLogService
{
    private readonly IApplicationDbContext _dbContext;

    public EmailLogService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogEmailAsync(string to, string subject, string body, bool success, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var emailLog = new Domain.Entities.EmailLog
        {
            ToEmail = to,
            Subject = subject,
            Body = body,
            Status = success ? "Sent" : "Failed",
            ErrorMessage = errorMessage,
            SentAt = success ? DateTime.UtcNow : null,
            EmailType = DetermineEmailType(subject)
        };

        _dbContext.EmailLogs.Add(emailLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string DetermineEmailType(string subject)
    {
        var lowerSubject = subject.ToLowerInvariant();
        if (lowerSubject.Contains("welcome") || lowerSubject.Contains("registration"))
            return "Welcome";
        if (lowerSubject.Contains("donation") || lowerSubject.Contains("receipt"))
            return "DonationReceipt";
        if (lowerSubject.Contains("password") || lowerSubject.Contains("reset"))
            return "PasswordReset";
        if (lowerSubject.Contains("invitation"))
            return "Invitation";
        return "General";
    }
}

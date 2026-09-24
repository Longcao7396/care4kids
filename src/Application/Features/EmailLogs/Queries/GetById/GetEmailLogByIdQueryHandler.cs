using GiveAID.Application.Features.EmailLogs.DTOs;

namespace GiveAID.Application.Features.EmailLogs.Queries.GetById;

/// <summary>
/// Handler for GetEmailLogByIdQuery.
/// </summary>
public class GetEmailLogByIdQueryHandler : IRequestHandler<GetEmailLogByIdQuery, EmailLogDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmailLogByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLogDto> Handle(GetEmailLogByIdQuery request, CancellationToken cancellationToken)
    {
        var emailLog = await _context.EmailLogs.FindAsync(new object[] { request.EmailLogId }, cancellationToken);

        if (emailLog == null)
        {
            throw new InvalidOperationException($"Email log with ID {request.EmailLogId} not found.");
        }

        return new EmailLogDto
        {
            EmailLogId = emailLog.EmailLogId,
            ToEmail = emailLog.ToEmail,
            Subject = emailLog.Subject,
            Category = emailLog.EmailType,
            Status = emailLog.Status,
            SentAt = emailLog.SentAt,
            ErrorMessage = emailLog.ErrorMessage,
            CreatedAt = emailLog.CreatedAt
        };
    }
}

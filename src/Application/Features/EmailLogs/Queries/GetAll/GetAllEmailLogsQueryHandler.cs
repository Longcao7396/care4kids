using GiveAID.Application.Features.EmailLogs.DTOs;

namespace GiveAID.Application.Features.EmailLogs.Queries.GetAll;

/// <summary>
/// Handler for GetAllEmailLogsQuery.
/// </summary>
public class GetAllEmailLogsQueryHandler : IRequestHandler<GetAllEmailLogsQuery, IEnumerable<EmailLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEmailLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmailLogDto>> Handle(GetAllEmailLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(e => e.EmailType == request.Category);
        }

        var logs = await _context.EmailLogs
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return logs.Select(e => new EmailLogDto
        {
            EmailLogId = e.EmailLogId,
            ToEmail = e.ToEmail,
            Subject = e.Subject,
            Category = e.EmailType,
            Status = e.Status,
            SentAt = e.SentAt,
            ErrorMessage = e.ErrorMessage,
            CreatedAt = e.CreatedAt
        });
    }
}

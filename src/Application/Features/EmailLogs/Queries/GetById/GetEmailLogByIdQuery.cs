using GiveAID.Application.Features.EmailLogs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.EmailLogs.Queries.GetById;

/// <summary>
/// Query to get an email log by ID.
/// </summary>
public class GetEmailLogByIdQuery : IRequest<EmailLogDto>
{
    public int EmailLogId { get; set; }
}

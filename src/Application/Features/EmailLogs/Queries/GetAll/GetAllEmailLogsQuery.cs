using GiveAID.Application.Features.EmailLogs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.EmailLogs.Queries.GetAll;

/// <summary>
/// Query to get all email logs.
/// </summary>
public class GetAllEmailLogsQuery : IRequest<IEnumerable<EmailLogDto>>
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

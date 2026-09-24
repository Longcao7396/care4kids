using GiveAID.Application.Features.Contacts.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Contacts.Commands.Submit;

/// <summary>
/// Command to submit a contact message.
/// </summary>
public class SubmitContactCommand : IRequest<ContactDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

using GiveAID.Application.Features.Users.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Users.Queries.GetById;

/// <summary>
/// Query to get a user by ID.
/// </summary>
public class GetUserByIdQuery : IRequest<UserAdminDto>
{
    public int UserId { get; set; }
}

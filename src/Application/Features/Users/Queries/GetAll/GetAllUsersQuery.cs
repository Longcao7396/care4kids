using GiveAID.Application.Features.Users.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Users.Queries.GetAll;

/// <summary>
/// Query to get all users.
/// </summary>
public class GetAllUsersQuery : IRequest<IEnumerable<UserAdminDto>>
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

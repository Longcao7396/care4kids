using GiveAID.Application.Features.Users.DTOs;
using GiveAID.Domain.Entities;
using GiveAID.Application.Services;
using MediatR;

namespace GiveAID.Application.Features.Users.Commands.CreateAdmin;

/// <summary>
/// Handler for CreateAdminCommand.
/// </summary>
public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, UserAdminDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAdminCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserAdminDto> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        // Check existing email
        if (_context.Users.Any(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Check existing username
        if (_context.Users.Any(u => u.Username == request.Username))
        {
            throw new InvalidOperationException("A user with this username already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            Role = request.Role,
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserAdminDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }
}

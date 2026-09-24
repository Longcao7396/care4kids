using GiveAID.Application.Features.Auth.DTOs;
using GiveAID.Application.Services;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand. Looks up the user by username (case-insensitive).
/// Email is no longer accepted as a login identifier — users must sign in
/// with their username.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var username = (request.Username ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new UnauthorizedAccessException("Username is required.");
        }

        // Look up user by username (case-insensitive).
        // Using ToLower() on both sides is SQL-translatable; string.Equals with
        // StringComparison.OrdinalIgnoreCase is NOT supported by EF Core's SQL provider.
        var user = await _context.Users
            .Where(u => u.IsActive && u.Username.ToLower() == username.ToLower())
            .FirstOrDefaultAsync(cancellationToken);

        // SECURITY FIX #3: Timing oracle mitigation
        // When user is NOT found, run a dummy password hash with the SAME cost 
        // to equalize timing. This prevents attackers from distinguishing 
        // "user not found" vs "wrong password" based on response time.
        if (user == null)
        {
            // Use a dummy hash to equalize timing with the BCrypt work factor
            _passwordHasher.Hash("dummy_password_for_timing_equalization");
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // SECURITY FIX #1: Enforce email verification before login
        // Block login for unverified users to prevent access until email is confirmed.
        if (!user.IsVerified)
        {
            throw new LoginFailedDueToUnverifiedEmailException(
                "Please verify your email before logging in. Check your inbox for the verification link.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var expiresAt = _jwtTokenService.GetTokenExpiration();
        var token = _jwtTokenService.GenerateToken(user.UserId, user.Email, user.Role, user.Username);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}

/// <summary>
/// Custom exception for login failures due to unverified email.
/// This allows the controller to distinguish this case and return
/// a specific error code (EMAIL_NOT_VERIFIED) to the frontend.
/// </summary>
public class LoginFailedDueToUnverifiedEmailException : Exception
{
    public LoginFailedDueToUnverifiedEmailException(string message) : base(message) { }
}

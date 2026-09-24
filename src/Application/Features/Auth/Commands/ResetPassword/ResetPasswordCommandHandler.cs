using GiveAID.Application.Services;
using GiveAID.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveAID.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand.
/// Validates the one-time token, resets the password, and marks the token as used.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException("Reset token is required.");
        }

        var token = request.Token.Trim();

        // L-01: Look up the password reset token
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (resetToken == null)
        {
            _logger.LogWarning("Invalid or expired password reset token attempted: {TokenPrefix}...",
                token.Length > 8 ? token[..8] : token);
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        var user = resetToken.User;
        if (user == null || !user.IsActive)
        {
            throw new InvalidOperationException("Invalid or expired reset token.");
        }

        // L-01: Mark the token as used (one-time use)
        resetToken.MarkAsUsed();

        // Update user's password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.VerificationToken = null;   // Clear any old email verification token too
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successful for user {UserId}", user.UserId);

        return true;
    }
}

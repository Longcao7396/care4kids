using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveAID.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Handler for ForgotPasswordCommand.
/// Generates a secure, single-use, time-limited password reset token
/// and sends a reset email to the user.
/// 
/// SECURITY FIX #4: Added exponential backoff rate limiting.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordResetThrottleService _throttleService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    // L-01: Configuration constants
    private const int TokenExpirationHours = 1;        // Token expires in 1 hour
    private const string ResetUrlBase = "https://giveaid.org/reset-password";

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailSender emailSender,
        IPasswordResetThrottleService throttleService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _throttleService = throttleService;
        _logger = logger;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
        {
            // L-01: Always return success to prevent email enumeration attacks
            return ForgotPasswordResult.Success();
        }

        // SECURITY FIX #4: Check rate limiting BEFORE any DB lookup
        var ipAddress = GetClientIpAddress();
        var throttleResult = _throttleService.CheckThrottle(email, ipAddress);
        
        if (throttleResult.IsThrottled)
        {
            _logger.LogWarning("Password reset request throttled for email {Email}: {Message}", 
                email, throttleResult.Message);
            return ForgotPasswordResult.Throttled(throttleResult.RetryAfterSeconds, throttleResult.Message);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.IsActive, cancellationToken);

        // L-01: Always return success to prevent email enumeration attacks
        if (user == null)
        {
            _logger.LogInformation("Forgot password request for non-existent email: {Email}", email);
            return ForgotPasswordResult.Success();
        }

        // L-01: Invalidate any existing unused tokens for this user
        // (creating a new token invalidates old ones)
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var existingToken in existingTokens)
        {
            existingToken.MarkAsUsed();
        }

        // L-01: Generate cryptographically secure token
        // Using Guid.NewGuid().ToString("N") gives 32 random hex chars (128 bits of entropy)
        var resetToken = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours);

        var passwordResetToken = new Domain.Entities.PasswordResetToken
        {
            UserId = user.UserId,
            Token = resetToken,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(passwordResetToken);
        await _context.SaveChangesAsync(cancellationToken);

        // L-01: Build reset URL — frontend parses ?token=xxx query param
        var resetUrl = $"{ResetUrlBase}?token={resetToken}";

        _logger.LogInformation(
            "Password reset token generated for user {UserId}, expires at {ExpiresAt}",
            user.UserId, expiresAt);

        // L-01: Send password reset email
        await _emailSender.SendPasswordResetEmailAsync(user.Email, resetToken, resetUrl);

        // Record the request for rate limiting
        _throttleService.RecordRequest(email, ipAddress);

        return ForgotPasswordResult.Success();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string GetClientIpAddress()
    {
        // This would typically be injected via IHttpContextAccessor
        // For now, return a default that can be overridden via context
        return "unknown";
    }
}

/// <summary>
/// Result type for forgot password operation.
/// </summary>
public class ForgotPasswordResult
{
    public bool IsSuccess { get; set; }
    public bool IsThrottled { get; set; }
    public int RetryAfterSeconds { get; set; }
    public string? Message { get; set; }

    public static ForgotPasswordResult Success() => new() { IsSuccess = true };
    
    public static ForgotPasswordResult Throttled(int retryAfterSeconds, string message) => new()
    {
        IsSuccess = false,
        IsThrottled = true,
        RetryAfterSeconds = retryAfterSeconds,
        Message = message
    };
}

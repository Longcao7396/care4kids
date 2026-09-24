using System.Collections.Concurrent;
using GiveAID.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GiveAID.Infrastructure.Services;

/// <summary>
/// In-memory password reset rate limiter with exponential backoff.
///
/// SECURITY FIX #4: Prevents password reset abuse by enforcing:
/// - Max 3 requests per email per hour
/// - Max 10 requests per IP per hour  
/// - Exponential backoff on consecutive requests (1min, 5min, 15min, 1hr)
///
/// Note: In-memory storage is suitable for single-instance deployments.
/// For multi-instance, use IDistributedCache (Redis) instead.
/// </summary>
public class PasswordResetThrottleService : IPasswordResetThrottleService
{
    private readonly ILogger<PasswordResetThrottleService> _logger;

    // Constants
    private const int MaxRequestsPerEmailPerHour = 3;
    private const int MaxRequestsPerIpPerHour = 10;
    private const int WindowSeconds = 3600; // 1 hour

    // Exponential backoff intervals in seconds
    private static readonly int[] BackoffIntervals = { 60, 300, 900, 3600 }; // 1min, 5min, 15min, 1hr

    // In-memory storage for tracking requests
    private static readonly ConcurrentDictionary<string, ThrottleEntry> _emailTrackers = new();
    private static readonly ConcurrentDictionary<string, ThrottleEntry> _ipTrackers = new();

    public PasswordResetThrottleService(ILogger<PasswordResetThrottleService> logger)
    {
        _logger = logger;
    }

    public PasswordResetThrottleResult CheckThrottle(string email, string ipAddress)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-WindowSeconds);

        // Normalize email to lowercase for consistent key lookup
        var normalizedEmail = email.ToLowerInvariant();

        // Check email-based throttle
        var emailKey = $"email:{normalizedEmail}";
        var emailEntry = GetOrCreateEntry(emailKey, _emailTrackers);
        CleanOldEntries(emailEntry, windowStart);

        var emailAttempts = emailEntry.Requests.Count(r => r > windowStart);
        if (emailAttempts >= MaxRequestsPerEmailPerHour)
        {
            var oldestRequest = emailEntry.Requests.LastOrDefault();
            var retryAfter = oldestRequest > DateTime.MinValue 
                ? (int)Math.Ceiling((oldestRequest.AddSeconds(WindowSeconds) - now).TotalSeconds)
                : WindowSeconds;

            _logger.LogWarning("Email {Email} exceeded rate limit: {Count}/{Max} requests per hour", 
                normalizedEmail, emailAttempts, MaxRequestsPerEmailPerHour);

            return new PasswordResetThrottleResult
            {
                IsThrottled = true,
                RetryAfterSeconds = Math.Max(1, retryAfter),
                AttemptCount = emailAttempts,
                Message = "Too many password reset attempts. Please try again later."
            };
        }

        // Check IP-based throttle
        var ipKey = $"ip:{ipAddress}";
        var ipEntry = GetOrCreateEntry(ipKey, _ipTrackers);
        CleanOldEntries(ipEntry, windowStart);

        var ipAttempts = ipEntry.Requests.Count(r => r > windowStart);
        if (ipAttempts >= MaxRequestsPerIpPerHour)
        {
            var oldestRequest = ipEntry.Requests.LastOrDefault();
            var retryAfter = oldestRequest > DateTime.MinValue 
                ? (int)Math.Ceiling((oldestRequest.AddSeconds(WindowSeconds) - now).TotalSeconds)
                : WindowSeconds;

            _logger.LogWarning("IP {IpAddress} exceeded rate limit: {Count}/{Max} requests per hour", 
                ipAddress, ipAttempts, MaxRequestsPerIpPerHour);

            return new PasswordResetThrottleResult
            {
                IsThrottled = true,
                RetryAfterSeconds = Math.Max(1, retryAfter),
                AttemptCount = ipAttempts,
                Message = "Too many password reset attempts from this location. Please try again later."
            };
        }

        // Check exponential backoff
        var backoffLevel = emailEntry.ConsecutiveFailures < BackoffIntervals.Length 
            ? emailEntry.ConsecutiveFailures 
            : BackoffIntervals.Length - 1;
        
        if (backoffLevel > 0 && emailEntry.LastFailure.HasValue)
        {
            var backoffInterval = BackoffIntervals[backoffLevel - 1];
            var timeSinceLastFailure = (now - emailEntry.LastFailure.Value).TotalSeconds;
            
            if (timeSinceLastFailure < backoffInterval)
            {
                var retryAfter = (int)(backoffInterval - timeSinceLastFailure);
                
                _logger.LogWarning("Email {Email} is in exponential backoff: level {Level}, retry in {Seconds}s", 
                    normalizedEmail, backoffLevel, retryAfter);

                return new PasswordResetThrottleResult
                {
                    IsThrottled = true,
                    RetryAfterSeconds = Math.Max(1, retryAfter),
                    AttemptCount = emailAttempts,
                    Message = $"Too many failed attempts. Please wait {retryAfter / 60} minutes before trying again."
                };
            }
        }

        return new PasswordResetThrottleResult
        {
            IsThrottled = false,
            RetryAfterSeconds = 0,
            AttemptCount = emailAttempts
        };
    }

    public void RecordRequest(string email, string ipAddress)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // Record in email tracker
        var emailKey = $"email:{normalizedEmail}";
        var emailEntry = GetOrCreateEntry(emailKey, _emailTrackers);
        emailEntry.Requests.Add(DateTime.UtcNow);
        emailEntry.LastRequest = DateTime.UtcNow;

        // Record in IP tracker
        var ipKey = $"ip:{ipAddress}";
        var ipEntry = GetOrCreateEntry(ipKey, _ipTrackers);
        ipEntry.Requests.Add(DateTime.UtcNow);
        ipEntry.LastRequest = DateTime.UtcNow;

        // Reset consecutive failures on successful request
        emailEntry.ConsecutiveFailures = 0;
        emailEntry.LastFailure = null;
    }

    private ThrottleEntry GetOrCreateEntry(string key, ConcurrentDictionary<string, ThrottleEntry> tracker)
    {
        return tracker.GetOrAdd(key, _ => new ThrottleEntry());
    }

    private void CleanOldEntries(ThrottleEntry entry, DateTime windowStart)
    {
        // Remove requests older than the window
        var toRemove = entry.Requests.Where(r => r < windowStart).ToList();
        foreach (var old in toRemove)
        {
            entry.Requests.Remove(old);
        }
    }

    private class ThrottleEntry
    {
        public List<DateTime> Requests { get; } = new();
        public DateTime LastRequest { get; set; }
        public int ConsecutiveFailures { get; set; }
        public DateTime? LastFailure { get; set; }
    }
}

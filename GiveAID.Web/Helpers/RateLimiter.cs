using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Simple in-process IP-based rate limiter for unauthenticated public
    /// endpoints (contact form, career applications). Prevents spam abuse
    /// from a single source. NOT suitable for multi-instance deployments —
    /// in that case switch to a distributed cache (Redis) or a proper API
    /// gateway rate limit (Cloudflare, Azure API Management).
    ///
    /// Algorithm: fixed-window counter keyed by (ip, endpoint). The counter
    /// resets every windowSeconds. If the count exceeds maxRequests within
    /// the window, returns true from IsLimited().
    ///
    /// Usage:
    ///     if (RateLimiter.IsLimited("contacts-submit", 5, 60)) {
    ///         return BadRequest("Too many requests. Please try again later.");
    ///     }
    /// </summary>
    public static class RateLimiter
    {
        private const string CacheKeyPrefix = "ratelimit::";

        public static bool IsLimited(string endpoint, int maxRequests, int windowSeconds)
        {
            // Identify the caller by IP. If behind a proxy, also try
            // X-Forwarded-For (first IP in the chain = original client).
            var ip = ResolveClientIp();
            var key = CacheKeyPrefix + endpoint + "::" + ip;
            var windowStart = DateTime.UtcNow.AddSeconds(-windowSeconds);

            // HttpRuntime.Cache is process-scoped, in-memory, and shared across
            // all requests on the same IIS worker. We store a ConcurrentQueue
            // of request timestamps and count entries newer than the window.
            var queue = HttpRuntime.Cache.Get(key) as ConcurrentQueue<DateTime>;
            if (queue == null)
            {
                queue = new ConcurrentQueue<DateTime>();
                HttpRuntime.Cache.Insert(
                    key,
                    queue,
                    null,
                    DateTime.UtcNow.AddSeconds(windowSeconds * 2),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }

            // Drop expired entries (older than the window).
            while (queue.TryPeek(out var oldest) && oldest < windowStart)
            {
                queue.TryDequeue(out _);
            }

            queue.Enqueue(DateTime.UtcNow);
            return queue.Count > maxRequests;
        }

        private static string ResolveClientIp()
        {
            var xff = HttpContext.Current?.Request?.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(xff))
            {
                // X-Forwarded-For can be a comma-separated list; client is first.
                var first = xff.Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(first)) return first;
            }
            return HttpContext.Current?.Request?.UserHostAddress ?? "unknown";
        }
    }
}

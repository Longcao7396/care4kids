using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Reflection;
using GiveAID.Web.Helpers;
using Xunit;

namespace GiveAID.Tests.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="RateLimiter"/>.
    ///
    /// Algorithm: fixed-window counter keyed by (endpoint, IP address).
    ///   • Under the limit  → <c>IsLimited</c> returns false.
    ///   • Exceeds the limit → <c>IsLimited</c> returns true.
    ///   • Different IPs    → tracked separately.
    ///
    /// Note: These tests cannot use HttpContext mocking in SDK-style projects
    /// targeting .NET Framework due to constructor compatibility issues.
    /// RateLimiter tests are simplified to verify basic counter behavior.
    /// </summary>
    public class RateLimiterTests
    {
        // ════════════════════════════════════════════════════════════════════
        // Basic counter behavior tests (using unique endpoints per test)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IsAllowed_UnderLimit_ReturnsFalse()
        {
            // With max=5, window=60s, making 3 requests should be fine.
            var endpoint = $"test-under-limit-{Guid.NewGuid()}";
            for (int i = 0; i < 3; i++)
            {
                var isLimited = RateLimiter.IsLimited(endpoint, 5, 60);
                Assert.False(isLimited,
                    $"Request {i + 1}/3 should NOT be rate-limited.");
            }
        }

        [Fact]
        public void IsAllowed_ExactlyAtLimit_ReturnsFalse()
        {
            var endpoint = $"rate-limit-at-limit-{Guid.NewGuid()}";

            // Make exactly maxRequests requests
            for (int i = 0; i < 5; i++)
            {
                var isLimited = RateLimiter.IsLimited(endpoint, 5, 60);
                Assert.False(isLimited,
                    $"Request {i + 1}/5 (at limit) should NOT be rate-limited.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Over-limit behaviour
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IsAllowed_ExceedsLimit_ReturnsTrue()
        {
            var endpoint = $"rate-limit-exceed-{Guid.NewGuid()}";

            // Fill the bucket
            for (int i = 0; i < 5; i++)
            {
                RateLimiter.IsLimited(endpoint, 5, 60);
            }

            // 6th request should be limited
            var isLimited = RateLimiter.IsLimited(endpoint, 5, 60);
            Assert.True(isLimited,
                "6th request with max=5 should be rate-limited.");
        }

        [Fact]
        public void IsAllowed_ExceedsLimitByMany_ReturnsTrue()
        {
            var endpoint = $"rate-limit-many-{Guid.NewGuid()}";

            // Make 20 requests against a limit of 5
            for (int i = 0; i < 20; i++)
            {
                RateLimiter.IsLimited(endpoint, 5, 60);
            }

            Assert.True(RateLimiter.IsLimited(endpoint, 5, 60));
        }

        // ════════════════════════════════════════════════════════════════════
        // Different endpoints tracked separately
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IsAllowed_DifferentEndpoints_TrackedSeparately()
        {
            var endpoint1 = $"contact-submit-{Guid.NewGuid()}";
            var endpoint2 = $"apply-career-{Guid.NewGuid()}";

            // Exhaust endpoint1
            for (int i = 0; i < 5; i++) RateLimiter.IsLimited(endpoint1, 5, 60);

            // endpoint2 should be unaffected
            var isLimited = RateLimiter.IsLimited(endpoint2, 5, 60);
            Assert.False(isLimited,
                "Different endpoint should have a separate counter.");
        }

        // ════════════════════════════════════════════════════════════════════
        // Edge cases
        // ════════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void IsAllowed_ZeroMaxRequests_AlwaysReturnsTrue(int count)
        {
            var endpoint = $"rate-limit-zero-{Guid.NewGuid()}";

            for (int i = 0; i < count; i++)
            {
                Assert.True(RateLimiter.IsLimited(endpoint, 0, 60),
                    $"Even the first request with max=0 should be limited.");
            }
        }

        [Fact]
        public void IsAllowed_WindowOfZero_TreatedAsInfiniteWindow()
        {
            // With windowSeconds=0, the windowStart = now (no expiry), so
            // every call increments the counter without ever resetting.
            var endpoint = $"rate-limit-window0-{Guid.NewGuid()}";

            for (int i = 0; i < 10; i++)
            {
                var result = RateLimiter.IsLimited(endpoint, 5, 0);
                if (i < 5) Assert.False(result, $"Request {i + 1} should not be limited.");
                else       Assert.True(result,  $"Request {i + 1} should be limited.");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Concurrency
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IsAllowed_ConcurrentRequestsFromSameEndpoint_DoesNotThrow()
        {
            var endpoint = $"rate-limit-concurrent-{Guid.NewGuid()}";
            const int threadCount = 20;

            var exceptions = new ConcurrentBag<Exception>();

            var threads = new System.Threading.Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        RateLimiter.IsLimited(endpoint, 5, 60);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Empty(exceptions);
        }
    }
}

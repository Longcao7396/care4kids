using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Simple in-memory cache wrapper using System.Runtime.Caching.
    /// Provides thread-safe caching with TTL (time-to-live) support.
    ///
    /// Thread-safety strategy:
    ///   - A per-key "stamp" is held in a ConcurrentDictionary while the factory
    ///     is running. Other threads that arrive for the same key wait on the
    ///     same stamp, so the factory executes exactly once per cache miss
    ///     (single-flight / coalescing pattern).
    ///   - The cache itself is MemoryCache.Default which is already thread-safe.
    ///   - Stamps are removed from the dictionary once the leading thread finishes,
    ///     so memory does not grow unboundedly under churn.
    /// </summary>
    public static class CacheHelper
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private static readonly ConcurrentDictionary<string, Stamp> _stamps
            = new ConcurrentDictionary<string, Stamp>(StringComparer.Ordinal);

        /// <summary>
        /// Get cached value or set if not exists (sync version).
        /// Thread-safe: concurrent callers for the same key share one factory call.
        /// </summary>
        public static T GetOrSet<T>(string key, Func<T> factory, int cacheMinutes = 5)
        {
            var cached = _cache.Get(key);
            if (cached != null)
            {
                return (T)cached;
            }

            // Single-flight: one thread runs the factory, others wait on its result.
            var stamp = _stamps.GetOrAdd(key, _ => new Stamp());
            try
            {
                stamp.Gate.Wait();
                try
                {
                    // Re-check after acquiring the gate — the leading thread may
                    // have populated the cache while we were waiting.
                    cached = _cache.Get(key);
                    if (cached != null) return (T)cached;

                    var value = factory();
                    if (value != null)
                    {
                        var policy = new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheMinutes)
                        };
                        _cache.Set(key, value, policy);
                    }
                    return value;
                }
                finally
                {
                    stamp.Gate.Release();
                }
            }
            finally
            {
                // Only the LAST waiter removes the stamp; otherwise another
                // concurrent caller could miss the dedupe and create a fresh stamp.
                if (stamp.Gate.CurrentCount == 1)
                {
                    _stamps.TryRemove(key, out _);
                }
            }
        }

        /// <summary>
        /// Get cached value or set if not exists (async version).
        /// </summary>
        public static async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int cacheMinutes = 5)
        {
            var cached = _cache.Get(key);
            if (cached != null)
            {
                return (T)cached;
            }

            var value = await factory();
            if (value != null)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheMinutes)
                };
                _cache.Set(key, value, policy);
            }

            return value;
        }

        /// <summary>
        /// Remove a specific cache key.
        /// </summary>
        public static void Remove(string key)
        {
            _cache.Remove(key);
            _stamps.TryRemove(key, out _);
        }

        /// <summary>
        /// Invalidate all statistics caches.
        /// </summary>
        public static void InvalidateStatistics()
        {
            Remove("stats_overview");
            Remove("stats_dashboard");
            Remove("stats_campaigns");
            Remove("stats_recent_donations");
        }

        /// <summary>
        /// Single-flight coordination primitive. SemaphoreSlim(1,1) acts as a
        /// per-key gate: the first thread to enter runs the factory, subsequent
        /// threads for the same key block on Wait() and then read the result.
        /// </summary>
        private sealed class Stamp
        {
            public readonly System.Threading.SemaphoreSlim Gate = new System.Threading.SemaphoreSlim(1, 1);
        }
    }
}

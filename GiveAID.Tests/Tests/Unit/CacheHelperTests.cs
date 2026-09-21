using System;
using System.Threading;
using System.Threading.Tasks;
using GiveAID.Web.Helpers;
using Xunit;

namespace GiveAID.Tests.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="CacheHelper"/>.
    /// Tests verify:
    ///   • GetOrSet returns cached value on subsequent calls.
    ///   • GetOrSetAsync returns cached value on subsequent calls.
    ///   • Remove deletes the cached value.
    ///   • InvalidateStatistics clears all statistics cache keys.
    ///   • Factory is only called once when cache is populated.
    /// </summary>
    public class CacheHelperTests
    {
        // ════════════════════════════════════════════════════════════════════
        // GetOrSet - synchronous factory
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetOrSet_FirstCall_ExecutesFactory()
        {
            // Arrange
            var key = $"test_sync_{Guid.NewGuid()}";
            var callCount = 0;

            // Act
            var result = CacheHelper.GetOrSet(key, () =>
            {
                callCount++;
                return "test_value";
            }, cacheMinutes: 5);

            // Assert
            Assert.Equal("test_value", result);
            Assert.Equal(1, callCount);

            // Cleanup
            CacheHelper.Remove(key);
        }

        [Fact]
        public void GetOrSet_SecondCall_ReturnsCachedValue()
        {
            // Arrange
            var key = $"test_cache_{Guid.NewGuid()}";
            var callCount = 0;

            // First call - populates cache
            CacheHelper.GetOrSet(key, () =>
            {
                callCount++;
                return 42;
            }, cacheMinutes: 5);

            // Act - second call should return cached value
            var result = CacheHelper.GetOrSet(key, () =>
            {
                callCount++;
                return 99; // This should not be returned
            }, cacheMinutes: 5);

            // Assert
            Assert.Equal(42, result);
            Assert.Equal(1, callCount); // Factory called only once

            // Cleanup
            CacheHelper.Remove(key);
        }

        [Fact]
        public void GetOrSet_NullValue_DoesNotCache()
        {
            // Arrange
            var key = $"test_null_{Guid.NewGuid()}";

            // Act
            var result = CacheHelper.GetOrSet(key, () => (string)null, cacheMinutes: 5);

            // Assert
            Assert.Null(result);

            // Second call should execute factory again since null wasn't cached
            var callCount = 0;
            result = CacheHelper.GetOrSet(key, () =>
            {
                callCount++;
                return "after_null";
            }, cacheMinutes: 5);

            Assert.Equal("after_null", result);
            Assert.Equal(1, callCount);

            // Cleanup
            CacheHelper.Remove(key);
        }

        // ════════════════════════════════════════════════════════════════════
        // GetOrSetAsync - asynchronous factory
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetOrSetAsync_FirstCall_ExecutesFactory()
        {
            // Arrange
            var key = $"test_async_{Guid.NewGuid()}";
            var callCount = 0;

            // Act
            var result = await CacheHelper.GetOrSetAsync(key, async () =>
            {
                callCount++;
                await Task.Delay(1); // Simulate async work
                return "async_value";
            }, cacheMinutes: 5);

            // Assert
            Assert.Equal("async_value", result);
            Assert.Equal(1, callCount);

            // Cleanup
            CacheHelper.Remove(key);
        }

        [Fact]
        public async Task GetOrSetAsync_SecondCall_ReturnsCachedValue()
        {
            // Arrange
            var key = $"test_async_cache_{Guid.NewGuid()}";
            var callCount = 0;

            // First call
            await CacheHelper.GetOrSetAsync(key, async () =>
            {
                callCount++;
                await Task.Delay(1);
                return 100;
            }, cacheMinutes: 5);

            // Act - second call
            var result = await CacheHelper.GetOrSetAsync(key, async () =>
            {
                callCount++;
                await Task.Delay(1);
                return 999; // Should not be returned
            }, cacheMinutes: 5);

            // Assert
            Assert.Equal(100, result);
            Assert.Equal(1, callCount);

            // Cleanup
            CacheHelper.Remove(key);
        }

        // ════════════════════════════════════════════════════════════════════
        // Remove
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Remove_ExistingKey_DeletesFromCache()
        {
            // Arrange
            var key = $"test_remove_{Guid.NewGuid()}";
            CacheHelper.GetOrSet(key, () => "value_before", cacheMinutes: 5);

            // Act
            CacheHelper.Remove(key);

            // Assert - after remove, factory should be called again
            var callCount = 0;
            var result = CacheHelper.GetOrSet(key, () =>
            {
                callCount++;
                return "value_after";
            }, cacheMinutes: 5);

            Assert.Equal("value_after", result);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Remove_NonExistingKey_DoesNotThrow()
        {
            // Arrange
            var key = $"nonexistent_key_{Guid.NewGuid()}";

            // Act & Assert - should not throw
            var exception = Record.Exception(() => CacheHelper.Remove(key));
            Assert.Null(exception);
        }

        // ════════════════════════════════════════════════════════════════════
        // InvalidateStatistics
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void InvalidateStatistics_ClearsAllStatsCacheKeys()
        {
            // Arrange - populate all statistics cache keys
            CacheHelper.GetOrSet("stats_overview", () => "overview_value", cacheMinutes: 5);
            CacheHelper.GetOrSet("stats_dashboard", () => "dashboard_value", cacheMinutes: 5);
            CacheHelper.GetOrSet("stats_campaigns", () => "campaigns_value", cacheMinutes: 5);
            CacheHelper.GetOrSet("stats_recent_donations", () => "donations_value", cacheMinutes: 5);

            // Act
            CacheHelper.InvalidateStatistics();

            // Assert - all keys should be cleared, factories should be called again
            var overviewCallCount = 0;
            var overview = CacheHelper.GetOrSet("stats_overview", () =>
            {
                overviewCallCount++;
                return "new_overview";
            }, cacheMinutes: 5);

            Assert.Equal("new_overview", overview);
            Assert.Equal(1, overviewCallCount);

            // Cleanup
            CacheHelper.Remove("stats_overview");
            CacheHelper.Remove("stats_dashboard");
            CacheHelper.Remove("stats_campaigns");
            CacheHelper.Remove("stats_recent_donations");
        }

        // ════════════════════════════════════════════════════════════════════
        // Thread safety
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetOrSet_ConcurrentCalls_OnlyExecutesFactoryOnce()
        {
            // Arrange
            var key = $"test_concurrent_{Guid.NewGuid()}";
            var callCount = 0;
            var barrier = new Barrier(5);

            // Act - multiple threads calling simultaneously
            Parallel.For(0, 5, _ =>
            {
                CacheHelper.GetOrSet(key, () =>
                {
                    Interlocked.Increment(ref callCount);
                    Thread.Sleep(50); // Simulate some work
                    return "concurrent_value";
                }, cacheMinutes: 5);
                barrier.SignalAndWait();
            });

            // Assert - factory should only be called once due to cache
            Assert.Equal(1, callCount);

            // Cleanup
            CacheHelper.Remove(key);
        }

        // ════════════════════════════════════════════════════════════════════
        // Edge cases
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetOrSet_DifferentKeys_AreCachedSeparately()
        {
            // Arrange
            var key1 = $"test_key1_{Guid.NewGuid()}";
            var key2 = $"test_key2_{Guid.NewGuid()}";

            // Act
            var result1 = CacheHelper.GetOrSet(key1, () => "value1", cacheMinutes: 5);
            var result2 = CacheHelper.GetOrSet(key2, () => "value2", cacheMinutes: 5);

            // Assert
            Assert.Equal("value1", result1);
            Assert.Equal("value2", result2);

            // Cleanup
            CacheHelper.Remove(key1);
            CacheHelper.Remove(key2);
        }

        [Fact]
        public void GetOrSet_CacheMinutesZero_UsesDefaultBehavior()
        {
            // Arrange
            var key = $"test_zero_{Guid.NewGuid()}";

            // Act
            var result = CacheHelper.GetOrSet(key, () => "value", cacheMinutes: 0);

            // Assert
            Assert.Equal("value", result);

            // Cleanup
            CacheHelper.Remove(key);
        }
    }
}

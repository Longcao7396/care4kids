using Microsoft.Extensions.Caching.Memory;
using GiveAID.Application.Services;
using Microsoft.Extensions.Logging;

namespace GiveAID.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry) where T : class
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = await factory();
        Set(key, value, expiry);
        return value;
    }

    public T? Get<T>(string key) where T : class
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan expiry) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };
        _cache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void InvalidateStatistics()
    {
        // IMemoryCache doesn't support pattern-based removal
        _logger.LogWarning("InvalidateStatistics called - IMemoryCache doesn't support pattern removal");
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }
}

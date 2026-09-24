namespace GiveAID.Application.Services;

/// <summary>
/// Interface for caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache or sets it using the factory function.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry) where T : class;

    /// <summary>
    /// Gets a value from cache synchronously.
    /// </summary>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in cache.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan expiry) where T : class;

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Invalidates all statistics-related cache entries.
    /// </summary>
    void InvalidateStatistics();

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    bool Exists(string key);
}

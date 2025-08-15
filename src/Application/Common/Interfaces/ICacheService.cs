namespace CleanArch.Application.Common.Interfaces;

/// <summary>
/// Provides caching capabilities for the application.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value if found, otherwise null.</returns>
    ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Gets a cached value by key, or creates and caches it using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not found in cache.</param>
    /// <param name="expiration">Cache expiration time. If null, uses default expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Cache expiration time. If null, uses default expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current version for a given prefix.
    /// </summary>
    /// <param name="prefix">The cache prefix (e.g., "users").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current version number.</returns>
    ValueTask<long> GetVersionAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments the version for a given prefix, effectively invalidating all cached entries with that prefix.
    /// </summary>
    /// <param name="prefix">The cache prefix (e.g., "users").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new version number after increment.</returns>
    ValueTask<long> InvalidateVersionAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a versioned cache key using the current version for the prefix.
    /// </summary>
    /// <param name="prefix">The cache prefix (e.g., "users").</param>
    /// <param name="key">The specific cache key (e.g., "all", "123").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A versioned cache key (e.g., "users:all:v5").</returns>
    ValueTask<string> BuildVersionedKeyAsync(string prefix, string key, CancellationToken cancellationToken = default);
}

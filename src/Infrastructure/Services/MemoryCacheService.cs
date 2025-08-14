using System.Collections.Concurrent;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Services;

/// <summary>
/// Memory cache implementation using IMemoryCache with ValueTask optimization.
///
/// ValueTask: Zero allocation for cache hits (synchronous), handles async factories for misses
/// Async interface: Enables Redis/distributed cache implementations without API changes
/// Key tracker: IMemoryCache doesn't expose keys, needed for prefix-based removal operations
/// </summary>
public sealed class MemoryCacheService(
    IMemoryCache memoryCache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<MemoryCacheService> logger
) : ICacheService
{
    /// <summary>
    /// Thread-safe dictionary to track cache keys for prefix-based operations.
    /// Uses byte as value type for minimal memory footprint (only key enumeration needed).
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _keyTracker = new();
    private readonly TimeSpan _defaultExpiration = cacheOptions.Value.DefaultTimeout;

    public ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = memoryCache.Get<T>(key);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", key);
        }
        else
        {
            logger.LogDebug("Cache miss for key: {CacheKey}", key);
        }

        return ValueTask.FromResult(cached);
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        // Try to get from cache first
        var cached = memoryCache.Get<T>(key);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return cached;
        }

        logger.LogDebug("Cache miss for key: {CacheKey}, creating new value", key);

        // Create new value using factory
        var value = await factory();
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value ?? throw new InvalidOperationException($"Factory method returned null for key: {key}");
    }

    public ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(10), // Sliding expiration of 10 minutes
            Priority = CacheItemPriority.Normal,
        };

        // Add removal callback to track keys for prefix-based operations.
        // NOTE: This callback is ESSENTIAL for MemoryCache because:
        // 1. IMemoryCache doesn't provide native key enumeration (unlike Redis SCAN)
        // 2. RemoveByPrefixAsync requires knowing all cached keys
        // 3. Automatic cleanup when items expire or are evicted due to memory pressure
        options.RegisterPostEvictionCallback(
            (cacheKey, cacheValue, evictionReason, state) =>
            {
                _keyTracker.TryRemove(cacheKey.ToString()!, out _);
                logger.LogDebug(
                    "Cache entry removed for key: {CacheKey}, reason: {EvictionReason}",
                    cacheKey,
                    evictionReason
                );
            }
        );

        memoryCache.Set(key, value, options);
        _keyTracker.TryAdd(key, 0);

        logger.LogDebug(
            "Cache set for key: {CacheKey} with expiration: {Expiration}",
            key,
            expiration ?? _defaultExpiration
        );

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);
            logger.LogDebug("Cache removed for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache for key: {CacheKey}", key);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var keysToRemove = _keyTracker
                .Keys.Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                memoryCache.Remove(key);
                _keyTracker.TryRemove(key, out _);
            }

            logger.LogDebug("Cache cleared for prefix: {Prefix}, removed {Count} keys", prefix, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }

        return ValueTask.CompletedTask;
    }
}

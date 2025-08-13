using System.Collections.Concurrent;
using System.Text.Json;
using CleanArch.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Services;

public sealed class RedisCacheService(IDistributedCache distributedCache, ILogger<MemoryCacheService> logger)
    : ICacheService
{
    /// <summary>
    /// Thread-safe dictionary to track cache keys for prefix-based operations.
    /// Uses byte as value type for minimal memory footprint (only key enumeration needed).
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _keyTracker = new();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = await distributedCache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cached);
        }
        else
        {
            logger.LogDebug("Cache miss for key: {CacheKey}", key);
            return null;
        }
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var cached = await distributedCache.GetStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", key);

            var obj = JsonSerializer.Deserialize<T>(cached);
            if (obj is not null)
                return obj;
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

    public async ValueTask SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(10),
        };

        var serialized = JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
        _keyTracker.TryAdd(key, 0);

        logger.LogDebug(
            "Cache set for key: {CacheKey} with expiration: {Expiration}",
            key,
            expiration ?? _defaultExpiration
        );

        // No eviction callback for distributed cache
        // Key tracking is handled above
        await ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            distributedCache.Remove(key);
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
                distributedCache.Remove(key);
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

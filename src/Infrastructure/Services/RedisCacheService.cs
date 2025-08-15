using System.Text.Json;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Services;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<RedisCacheService> logger
) : ICacheService
{
    // NOTE: Key tracking removed for Redis implementation.
    // Reason: Redis supports native SCAN command for pattern-based key operations,
    // making manual key tracking unnecessary and inefficient.
    // Use Redis SCAN with pattern matching for RemoveByPrefixAsync instead.

    private readonly TimeSpan _defaultExpiration = cacheOptions.Value.DefaultTimeout;

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

        logger.LogDebug(
            "Cache set for key: {CacheKey} with expiration: {Expiration}",
            key,
            expiration ?? _defaultExpiration
        );

        // No eviction callback for distributed cache
        // Key tracking is handled above
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            distributedCache.Remove(key);
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
        // Note: With key-versioning approach, we don't need to physically remove old cache entries.
        // Old versioned keys will naturally expire via TTL and become unused when version increments.
        // This is more efficient and cluster-safe than scanning and deleting keys.

        logger.LogDebug("RemoveByPrefix called for: {Prefix} - using version invalidation instead", prefix);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<long> GetVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var versionKey = $"{prefix}:version";
            var version = await distributedCache.GetStringAsync(versionKey, cancellationToken);

            if (version is not null && long.TryParse(version, out var result))
            {
                return result;
            }

            logger.LogDebug("Version key {VersionKey} not found, returning default version 1", versionKey);
            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting version for prefix: {Prefix}", prefix);
            return 1; // Default version
        }
    }

    public async ValueTask<long> InvalidateVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var versionKey = $"{prefix}:version";

            // Get current version
            var currentVersionStr = await distributedCache.GetStringAsync(versionKey, cancellationToken);
            var currentVersion = long.TryParse(currentVersionStr, out var parsed) ? parsed : 0;
            var newVersion = currentVersion + 1;

            // Set new version with 30 days expiration
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) };

            await distributedCache.SetStringAsync(versionKey, newVersion.ToString(), options, cancellationToken);

            logger.LogDebug("Incremented version for prefix: {Prefix} to {Version}", prefix, newVersion);
            return newVersion;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating version for prefix: {Prefix}", prefix);
            return 1; // Fallback
        }
    }

    public async ValueTask<string> BuildVersionedKeyAsync(
        string prefix,
        string key,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var version = await GetVersionAsync(prefix, cancellationToken);
        return $"{prefix}:{key}:v{version}";
    }
}

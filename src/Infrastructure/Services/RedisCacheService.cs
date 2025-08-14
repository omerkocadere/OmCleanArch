using System.Text.Json;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CleanArch.Infrastructure.Services;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
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
        await ValueTask.CompletedTask;
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

    public async ValueTask RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var database = connectionMultiplexer.GetDatabase();
            var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
            var pattern = $"{prefix}*";
            var keysToDelete = new List<RedisKey>();

            // Use Redis SCAN command to find keys matching the pattern
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(key);

                // Process in batches to avoid memory issues with large key sets
                if (keysToDelete.Count >= 100)
                {
                    await database.KeyDeleteAsync([.. keysToDelete]);
                    logger.LogDebug("Deleted batch of {Count} keys with prefix: {Prefix}", keysToDelete.Count, prefix);
                    keysToDelete.Clear();
                }
            }

            // Delete remaining keys
            if (keysToDelete.Count > 0)
            {
                await database.KeyDeleteAsync(keysToDelete.ToArray());
                logger.LogDebug(
                    "Deleted final batch of {Count} keys with prefix: {Prefix}",
                    keysToDelete.Count,
                    prefix
                );
            }

            logger.LogDebug("Cache cleared for prefix: {Prefix}", prefix);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
        }
    }
}

// TODO 1: How to add RegisterPostEvictionCallback for distributed cache?
// TODO 2: Track cached entity in DbContext
// TODO 3: Read default timeout and cache provider (Redis or MemoryCache) from appsettings

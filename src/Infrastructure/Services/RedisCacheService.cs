using System.Text.Json;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Services;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<RedisCacheService> logger,
    Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache
) : ICacheService
{
    private readonly TimeSpan _defaultExpiration = cacheOptions.Value.DefaultTimeout;

    public async ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = await distributedCache.GetStringAsync(key, cancellationToken);
        return cached is not null ? JsonSerializer.Deserialize<T>(cached) : null;
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
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async ValueTask<long> InvalidateVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        try
        {
            var versionKey = $"{prefix}:version";
            var currentVersion = await GetVersionAsync(prefix, cancellationToken);
            var newVersion = currentVersion + 1;

            // Keep 30 days - version key loss would cause data consistency issues
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) };
            await distributedCache.SetStringAsync(versionKey, newVersion.ToString(), options, cancellationToken);

            memoryCache.Set(
                versionKey,
                newVersion,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }
            );

            return newVersion;
        }
        catch (Exception ex)
        {
            // If version increment fails, return safe fallback to prevent cache inconsistency
            logger.LogWarning(ex, "Failed to increment version for prefix {Prefix}, using fallback", prefix);
            return 1;
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

    public async ValueTask<long> GetVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var versionKey = $"{prefix}:version";

        if (memoryCache.TryGetValue(versionKey, out var cachedObj) && cachedObj is long cachedVersion)
        {
            return cachedVersion;
        }

        try
        {
            var version = await distributedCache.GetStringAsync(versionKey, cancellationToken);
            if (version is not null && long.TryParse(version, out var result))
            {
                memoryCache.Set(
                    versionKey,
                    result,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2) }
                );
                return result;
            }

            memoryCache.Set(
                versionKey,
                1L,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2) }
            );
            return 1;
        }
        catch (Exception ex)
        {
            // Network/serialization failure - return safe default
            logger.LogWarning(ex, "Failed to get version for prefix {Prefix}, using default", prefix);
            return 1;
        }
    }
}

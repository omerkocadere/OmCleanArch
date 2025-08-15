using System.Text.Json;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
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
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // prefer async remove to avoid blocking calls
            await distributedCache.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Cache removed for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache for key: {CacheKey}", key);
        }
    }

    public async ValueTask<long> InvalidateVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var versionKey = $"{prefix}:version";

            // Determine current version via GetVersionAsync so defaults and local cache are respected
            var currentVersion = await GetVersionAsync(prefix, cancellationToken);
            var newVersion = currentVersion + 1;

            // Set new version with 30 days expiration
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) };

            await distributedCache.SetStringAsync(versionKey, newVersion.ToString(), options, cancellationToken);

            // Update local cached version for this prefix so subsequent reads return the new value
            try
            {
                memoryCache.Set(
                    versionKey,
                    newVersion,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }
                );
            }
            catch (Exception memEx)
            {
                logger.LogDebug(memEx, "Failed to update local version cache for {VersionKey}", versionKey);
            }

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

    public async ValueTask<long> GetVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var versionKey = $"{prefix}:version";
            // Try in-memory cache first to avoid frequent distributed calls under high load
            if (memoryCache.TryGetValue(versionKey, out var cachedObj) && cachedObj is long cachedVersion)
            {
                logger.LogDebug(
                    "Returning in-memory cached version for {VersionKey}: {Version}",
                    versionKey,
                    cachedVersion
                );
                return cachedVersion;
            }

            var version = await distributedCache.GetStringAsync(versionKey, cancellationToken);

            if (version is not null && long.TryParse(version, out var result))
            {
                // Cache locally for a short period to reduce distributed calls
                try
                {
                    memoryCache.Set(
                        versionKey,
                        result,
                        new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }
                    );
                }
                catch (Exception memEx)
                {
                    logger.LogDebug(memEx, "Failed to set local version cache for {VersionKey}", versionKey);
                }

                return result;
            }

            logger.LogDebug("Version key {VersionKey} not found, returning default version 1", versionKey);
            // Cache default version too to avoid hammering underlying store
            try
            {
                memoryCache.Set(
                    versionKey,
                    1L,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }
                );
            }
            catch (Exception memEx)
            {
                logger.LogDebug(memEx, "Failed to set local default version cache for {VersionKey}", versionKey);
            }

            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting version for prefix: {Prefix}", prefix);
            return 1; // Default version
        }
    }
}

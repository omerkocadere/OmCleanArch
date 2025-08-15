using System.Collections.Concurrent;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Services;

/// <summary>
/// Memory cache implementation using IMemoryCache with ValueTask optimization.
///
/// ValueTask: Zero allocation for cache hits (synchronous), handles async factories for misses
/// Async interface: Enables Redis/distributed cache implementations without API changes
/// Key tracker: IMemoryCache doesn't expose keys, needed for prefix-based removal operations
/// </summary>
public sealed class MemoryCacheService(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions) : ICacheService
{
    /// <summary>
    /// Thread-safe dictionary to track cache keys for prefix-based operations.
    /// Uses byte as value type for minimal memory footprint (only key enumeration needed).
    /// </summary>
    private readonly ConcurrentDictionary<string, byte> _keyTracker = new();

    /// <summary>
    /// Thread-safe dictionary to track version numbers for cache prefixes.
    /// Used for key-versioning invalidation strategy.
    /// </summary>
    private readonly ConcurrentDictionary<string, long> _versionTracker = new();

    private readonly TimeSpan _defaultExpiration = cacheOptions.Value.DefaultTimeout;

    public ValueTask<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cached = memoryCache.Get<T>(key);
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
        var cached = memoryCache.Get<T>(key);
        if (cached is not null)
        {
            return cached;
        }

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
            }
        );

        memoryCache.Set(key, value, options);
        _keyTracker.TryAdd(key, 0);

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        memoryCache.Remove(key);
        _keyTracker.TryRemove(key, out _);

        return ValueTask.CompletedTask;
    }

    public ValueTask<long> GetVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        var versionKey = $"{prefix}:version";
        var version = _versionTracker.GetOrAdd(versionKey, 1);
        return ValueTask.FromResult(version);
    }

    public ValueTask<long> InvalidateVersionAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var versionKey = $"{prefix}:version";
        var newVersion = _versionTracker.AddOrUpdate(versionKey, 2, (key, oldValue) => oldValue + 1);

        return ValueTask.FromResult(newVersion);
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

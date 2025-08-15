using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Common.Behaviours;

/// <summary>
/// Caching behavior for query handlers.
/// Implements caching for queries that implement ICacheableQuery.
/// </summary>
public sealed class CachingBehaviour<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehaviour<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // Only cache queries that implement ICacheableQuery
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next(cancellationToken);
        }

        // Build versioned cache key for cluster-safe invalidation
        var baseCacheKey = cacheableQuery.CacheKey;
        var prefix = ExtractPrefix(baseCacheKey);
        var key = ExtractKey(baseCacheKey, prefix);
        var versionedCacheKey = await cacheService.BuildVersionedKeyAsync(prefix, key, cancellationToken);

        logger.LogDebug(
            "Checking cache for versioned key: {CacheKey} (base: {BaseCacheKey})",
            versionedCacheKey,
            baseCacheKey
        );

        // Try to get from cache with error handling
        try
        {
            var cachedResponse = await cacheService.GetAsync<TResponse>(versionedCacheKey, cancellationToken);
            if (cachedResponse is not null)
            {
                logger.LogDebug("Cache hit for key: {CacheKey}", versionedCacheKey);
                return cachedResponse;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to get from cache for key: {CacheKey}. Proceeding with database query.",
                versionedCacheKey
            );
        }

        logger.LogDebug("Cache miss for key: {CacheKey}", versionedCacheKey);

        // Execute the actual handler
        var response = await next(cancellationToken);

        // Cache the response if it's successful with error handling
        if (ShouldCacheResponse(response))
        {
            try
            {
                await cacheService.SetAsync(versionedCacheKey, response, cacheableQuery.Expiration, cancellationToken);

                logger.LogDebug(
                    "Cached response for key: {CacheKey} with expiration: {Expiration}",
                    versionedCacheKey,
                    cacheableQuery.Expiration
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to set cache for key: {CacheKey}. Request completed successfully.",
                    versionedCacheKey
                );
            }
        }

        return response;
    }

    /// <summary>
    /// Extracts the prefix from a cache key (e.g., "users:all" -> "users").
    /// </summary>
    private static string ExtractPrefix(string cacheKey)
    {
        var colonIndex = cacheKey.IndexOf(':');
        return colonIndex > 0 ? cacheKey[..colonIndex] : cacheKey;
    }

    /// <summary>
    /// Extracts the key part after prefix (e.g., "users:all" -> "all").
    /// </summary>
    private static string ExtractKey(string cacheKey, string prefix)
    {
        return cacheKey.StartsWith($"{prefix}:") ? cacheKey[(prefix.Length + 1)..] : cacheKey;
    }

    private static bool ShouldCacheResponse(TResponse response)
    {
        // Don't cache failed results
        if (response is IOperationResult result)
        {
            return result.IsSuccess;
        }

        // Cache non-null responses
        return response is not null;
    }
}

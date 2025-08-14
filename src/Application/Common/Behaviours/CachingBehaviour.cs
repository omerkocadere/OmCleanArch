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

        var cacheKey = cacheableQuery.CacheKey;
        logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

        // Try to get from cache with error handling
        try
        {
            var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse is not null)
            {
                logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return cachedResponse;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to get from cache for key: {CacheKey}. Proceeding with database query.",
                cacheKey
            );
        }

        logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Execute the actual handler
        var response = await next(cancellationToken);

        // Cache the response if it's successful with error handling
        if (ShouldCacheResponse(response))
        {
            try
            {
                await cacheService.SetAsync(cacheKey, response, cacheableQuery.Expiration, cancellationToken);

                logger.LogDebug(
                    "Cached response for key: {CacheKey} with expiration: {Expiration}",
                    cacheKey,
                    cacheableQuery.Expiration
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to set cache for key: {CacheKey}. Request completed successfully.",
                    cacheKey
                );
            }
        }

        return response;
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

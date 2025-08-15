using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Common.Behaviours;

/// <summary>
/// Cache invalidation behavior for command handlers.
/// Automatically invalidates cache entries when entities are modified.
/// </summary>
public sealed class CacheInvalidationBehaviour<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CacheInvalidationBehaviour<TRequest, TResponse>> logger
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
        // Execute the command first
        var response = await next(cancellationToken);

        // Only invalidate cache for commands
        if (request is not ICommandMarker)
        {
            return response;
        }

        // Check if the response indicates success
        if (!IsSuccessfulResponse(response))
        {
            return response;
        }

        // Invalidate cache based on the command type
        await InvalidateCacheAsync(request, cancellationToken);

        return response;
    }

    private static bool IsSuccessfulResponse(TResponse response)
    {
        if (response is IOperationResult result)
        {
            return result.IsSuccess;
        }

        return response is not null;
    }

    private async Task InvalidateCacheAsync(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var requestType = request.GetType();
            var requestNamespace = requestType.Namespace ?? string.Empty;

            // User-related operations
            if (requestNamespace.Contains("Users"))
            {
                await InvalidateUserCacheAsync(cancellationToken);
            }

            // Add other entity cache invalidations here as needed
            // Example: if (requestNamespace.Contains("TodoItems"))
            // {
            //     await InvalidateTodoItemsCacheAsync(cancellationToken);
            // }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the request
            logger.LogError(ex, "Failed to invalidate cache for request {RequestType}", request.GetType().Name);
        }
    }

    private async Task InvalidateUserCacheAsync(CancellationToken cancellationToken)
    {
        // Use key versioning instead of RemoveByPrefix for cluster-safe invalidation
        var newVersion = await cacheService.InvalidateVersionAsync("users", cancellationToken);

        logger.LogDebug("Invalidated user-related cache entries by incrementing version to {Version}", newVersion);
    }
}

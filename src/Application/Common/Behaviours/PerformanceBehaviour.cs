using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    TimeProvider timeProvider
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int LongRunningThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var startTime = timeProvider.GetTimestamp();

        var response = await next(cancellationToken);

        var elapsed = timeProvider.GetElapsedTime(startTime);
        var elapsedMilliseconds = elapsed.TotalMilliseconds;

        if (elapsedMilliseconds > LongRunningThresholdMs)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                requestName,
                elapsedMilliseconds,
                request
            );
        }

        return response;
    }
}

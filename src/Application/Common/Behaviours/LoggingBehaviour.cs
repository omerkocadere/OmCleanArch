using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CleanArch.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse>(ILogger<TRequest> logger, IUserContext user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IOperationResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity("Request Logging Started");

        activity?.SetTag("logging.request.type", typeof(TRequest).Name);
        activity?.SetTag("logging.user.id", user.UserId);

        var requestName = typeof(TRequest).Name;
        var userId = user.UserId;

        logger.LogInformation("Processing request: {Name} {@UserId} {@Request}", requestName, userId, request);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
            {
                logger.LogError("Completed request {RequestName} with error", requestName);
            }
        }

        return result;
    }
}

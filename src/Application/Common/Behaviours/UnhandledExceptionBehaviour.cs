using Microsoft.Extensions.Logging;

namespace CleanArch.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex) when (LogException(ex, request))
        {
            throw;
        }
    }

    private bool LogException<T>(Exception ex, T request)
    {
        var requestName = typeof(T).Name;
        logger.LogError(ex, "Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
        return false; // Never handle, always rethrow
    }
}

using Microsoft.AspNetCore.Diagnostics;

namespace CleanArch.Web.Api.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var exceptionType = exception.GetType();
        var path = httpContext.Request.Path;
        var traceId = httpContext.TraceIdentifier;
        var innerException = exception.InnerException?.ToString();

        logger.LogError(
            exception,
            "Unhandled exception occurred. Type: {ExceptionType}, Path: {Path}, TraceIdentifier: {TraceId}, InnerException: {InnerException}",
            exceptionType.Name,
            path,
            traceId,
            innerException
        );

        var showStackTrace = env.IsDevelopment() || env.IsEnvironment("Docker");
        var problemDetails = Results.Problem(
            detail: showStackTrace ? exception.StackTrace?.ToString() : exception.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );

        await problemDetails.ExecuteAsync(httpContext);

        return true;
    }
}

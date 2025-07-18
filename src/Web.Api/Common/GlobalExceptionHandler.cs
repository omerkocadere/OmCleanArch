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
        logger.LogInformation("showStackTrace: {ShowStackTrace}", showStackTrace);
        logger.LogInformation("showStackTrace: {ShowStackTrace}", exception.ToString());
        var statusCode =
            (int?)exceptionType.GetProperty("StatusCode")?.GetValue(exception)
            ?? StatusCodes.Status500InternalServerError;

        var title = exceptionType.GetProperty("Title")?.GetValue(exception)?.ToString() ?? exception.Message;

        var problemDetails = Results.Problem(
            title: title,
            detail: showStackTrace ? exception.ToString() : exception.Message,
            statusCode: statusCode
        );

        await problemDetails.ExecuteAsync(httpContext);

        return true;
    }
}

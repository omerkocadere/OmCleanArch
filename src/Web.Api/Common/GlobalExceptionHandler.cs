using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Api.Common;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment env
) : IExceptionHandler
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
            "OmClenaArch unhandled exception. Type: {ExceptionType}, Path: {Path}, TraceIdentifier: {TraceId}, InnerException: {InnerException}",
            exceptionType.Name,
            path,
            traceId,
            innerException
        );

        var showStackTrace = env.IsDevelopment();
        httpContext.Response.StatusCode =
            (int?)exceptionType.GetProperty("StatusCode")?.GetValue(exception)
            ?? StatusCodes.Status500InternalServerError;

        var title = exceptionType.GetProperty("Title")?.GetValue(exception)?.ToString();

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Title = title,
                    Detail = showStackTrace ? exception.ToString() : exception.Message,
                },
            }
        );
    }
}

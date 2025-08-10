using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArch.Web.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests,
                        "Too Many Requests",
                        detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds."
                    );

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
                }
            };

            options.AddFixedWindowLimiter(
                "fixed",
                opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                }
            );

            options.AddPolicy(
                "per-user",
                httpContext =>
                {
                    string? userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        return RateLimitPartition.GetTokenBucketLimiter(
                            userId,
                            _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 7,
                                TokensPerPeriod = 2,
                                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            }
                        );
                    }

                    return RateLimitPartition.GetFixedWindowLimiter(
                        "anonymous",
                        _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(1) }
                    );
                }
            );
        });

        return services;
    }
}
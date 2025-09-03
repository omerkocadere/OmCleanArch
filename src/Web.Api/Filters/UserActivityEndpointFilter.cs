using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Members.Commands.UpdateUserActivity;

namespace CleanArch.Web.Api.Filters;

/// <summary>
/// Endpoint filter that tracks user activity using fire-and-forget pattern.
/// Follows Microsoft's recommended best practices for ASP.NET Core fire-and-forget scenarios.
/// </summary>
public class UserActivityEndpointFilter(
    IMediator mediator,
    IUserContext userContext,
    ILogger<UserActivityEndpointFilter> logger
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Execute the endpoint first
        var result = await next(context);

        // Fire-and-forget user activity update
        if (context.HttpContext.User.Identity?.IsAuthenticated == true && userContext.UserId.HasValue)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await mediator.Send(new UpdateUserActivityCommand());
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to update user activity for user {UserId}", userContext.UserId.Value);
                }
            });
        }

        return result;
    }
}

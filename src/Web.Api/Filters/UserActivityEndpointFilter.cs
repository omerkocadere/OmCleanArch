using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Members.Commands.UpdateUserActivity;

namespace CleanArch.Web.Api.Filters;

/// <summary>
/// Endpoint filter that tracks user activity using fire-and-forget pattern.
/// Uses proper DI scope management to avoid DbContext disposal issues.
/// </summary>
public class UserActivityEndpointFilter(
    IServiceScopeFactory serviceScopeFactory,
    ICurrentUser userContext,
    ILogger<UserActivityEndpointFilter> logger
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Execute the endpoint first
        var result = await next(context);

        // Fire-and-forget user activity update with proper DI scope
        if (context.HttpContext.User.Identity?.IsAuthenticated == true && userContext.UserId.HasValue)
        {
            var userId = userContext.UserId.Value; // Capture before background thread

            _ = Task.Run(async () =>
            {
                try
                {
                    // Create new scope for background thread - Microsoft's recommended pattern
                    await using var scope = serviceScopeFactory.CreateAsyncScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new UpdateUserActivityCommand(userId));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to update user activity for user {UserId}", userId);
                }
            });
        }

        return result;
    }
}

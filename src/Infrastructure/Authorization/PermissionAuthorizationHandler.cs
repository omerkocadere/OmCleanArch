using CleanArch.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        if (context.User is { Identity.IsAuthenticated: true })
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

            Guid? userId = context.User.GetUserId();
            if (userId is null)
            {
                return;
            }

            HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId.Value);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}

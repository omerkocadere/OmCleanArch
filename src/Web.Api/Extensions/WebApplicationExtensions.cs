using System.Reflection;
using Asp.Versioning;
using CleanArch.Web.Api.Filters;

namespace CleanArch.Web.Api.Extensions;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GroupName ?? group.GetType().Name;

        // Check if the group needs versioning
        if (group is IVersionedEndpointGroup versionedGroup)
        {
            // Create API version set for versioned groups
            var apiVersionSetBuilder = app.NewApiVersionSet();

            foreach (var version in versionedGroup.SupportedVersions)
            {
                apiVersionSetBuilder.HasApiVersion(new ApiVersion(version));
            }

            var apiVersionSet = apiVersionSetBuilder.ReportApiVersions().Build();

            return app.MapGroup($"/api/v{{version:apiVersion}}/{groupName}")
                .WithApiVersionSet(apiVersionSet)
                .WithGroupName(groupName)
                .WithTags(groupName);
        }
        else
        {
            // Standard non-versioned route for regular groups
            return app.MapGroup($"/api/{groupName}").WithGroupName(groupName).WithTags(groupName);
        }
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes().Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app.MapGroup(instance));
            }
        }

        return app;
    }

    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }

    /// <summary>
    /// Adds user activity tracking endpoint filter to all endpoints in the route group.
    /// Best practice: Use endpoint filters instead of middleware for endpoint-specific logic.
    /// This approach is more efficient as it only runs on specific endpoints rather than all requests.
    /// </summary>
    public static RouteGroupBuilder AddUserActivityTracking(this RouteGroupBuilder routeGroupBuilder)
    {
        return routeGroupBuilder.AddEndpointFilter<UserActivityEndpointFilter>();
    }
}

using System.Reflection;

namespace CleanArch.Web.Api.Extensions;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group, string? groupName = null)
    {
        var className = group.GetType().Name;
        var resolvedGroupName = groupName != null ? groupName + "-" + className : className;

        return app.MapGroup($"/api/{resolvedGroupName}").WithGroupName(resolvedGroupName).WithTags(resolvedGroupName);
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
                instance.Map(app);
            }
        }

        return app;
    }
}

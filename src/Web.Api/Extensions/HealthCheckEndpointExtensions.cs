using Microsoft.AspNetCore.Builder;

namespace CleanArch.Web.Api.Extensions;

public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Maps the health check endpoint conditionally or with custom logic.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <returns>The WebApplication instance.</returns>
    public static WebApplication MapConditionalHealthChecks(this WebApplication app)
    {
        // You can add custom logic here if needed (e.g., role-based, environment-based, etc.)
        app.MapHealthChecks("/health");
        return app;
    }
}

using CleanArch.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Web.Api;

public static class DependencyInjection
{
    public static void AddWebServices(this IServiceCollection services)
    {
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true
        );

        // Registers services required for minimal API endpoint discovery and Swagger/OpenAPI documentation generation.
        services.AddEndpointsApiExplorer();

        // This service relies on the metadata collected by AddEndpointsApiExplorer() to build a comprehensive OpenAPI (Swagger) specification document,
        services.AddOpenApiDocument();
    }
}

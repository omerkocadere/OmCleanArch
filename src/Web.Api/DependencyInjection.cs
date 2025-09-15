using Asp.Versioning;
using CleanArch.Infrastructure.Data;
using CleanArch.Web.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace CleanArch.Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(configure =>
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            }
        );

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

        // Add API Versioning - Only affects endpoints implementing IVersionedEndpointGroup
        services
            .AddApiVersioning(options =>
            {
                // Set default version to v1 when no version is specified
                options.DefaultApiVersion = new ApiVersion(1);
                // Use default version when client doesn't specify one
                options.AssumeDefaultVersionWhenUnspecified = true;
                // Include supported versions in response headers (api-supported-versions)
                options.ReportApiVersions = true;
                // Accept version from multiple sources: URL, query string, or header
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(), // /api/v1/members
                    new QueryStringApiVersionReader("version"), // ?version=1
                    new HeaderApiVersionReader("X-Version") // X-Version: 1
                );
            })
            .AddApiExplorer(options =>
            {
                // Format version groups as 'v1', 'v2' etc. in Swagger
                options.GroupNameFormat = "'v'VVV";
                // Replace {version} placeholder in URLs with actual version
                options.SubstituteApiVersionInUrl = true;
            });

        // Registers services required for minimal API endpoint discovery and Swagger/OpenAPI documentation generation.
        services.AddEndpointsApiExplorer();

        // This service relies on the metadata collected by AddEndpointsApiExplorer() to build a comprehensive OpenAPI (Swagger) specification document,
        services.AddOpenApiDocument(
            (configure, sp) =>
            {
                configure.Title = "JasonCA API";

                // Add JWT
                configure.AddSecurity(
                    "JWT",
                    [],
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Type into the textbox: Bearer {your JWT token}.",
                    }
                );

                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            }
        );

        // Add typed HttpClient for Dummy API
        services.AddHttpClient<DummyApiClient>(
            (serviceProvider, client) =>
            {
                var baseUrl = configuration["ApiClients:DummyApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiClients:DummyApiBaseUrl config is missing!");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }
        );

        // Add SignalR and related services
        services.AddSignalR();
        services.AddSingleton<IPresenceTracker, PresenceTracker>();

        return services;
    }
}

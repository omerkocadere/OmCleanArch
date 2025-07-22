using CleanArch.Infrastructure.Data;
using CleanArch.Web.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        // JWT Authentication
        // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer(options =>
        //     {
        //         options.TokenValidationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = new SymmetricSecurityKey(
        //                 Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))),
        //             ValidateIssuer = true,
        //             ValidIssuer = configuration["Jwt:Issuer"] ?? "CleanArch",
        //             ValidateAudience = true,
        //             ValidAudience = configuration["Jwt:Audience"] ?? "CleanArch",
        //             ValidateLifetime = true,
        //             ClockSkew = TimeSpan.Zero
        //         };
        //     });

        services.AddAuthorization();

        // Registers services required for minimal API endpoint discovery and Swagger/OpenAPI documentation generation.
        services.AddEndpointsApiExplorer();

        // This service relies on the metadata collected by AddEndpointsApiExplorer() to build a comprehensive OpenAPI (Swagger) specification document,
        services.AddOpenApiDocument();

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

        return services;
    }
}

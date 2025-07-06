using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection ConfigureOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var options = configuration
            .GetSection(OpenTelemetryOptions.SectionName)
            .Get<OpenTelemetryOptions>();

        var otlpEndpoint = options?.OtlpEndpoint;

        if (string.IsNullOrEmpty(otlpEndpoint))
        {
            throw new InvalidOperationException("OTLP endpoint is not configured.");
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("OmCleanArch"))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            });

        return services;
    }
}

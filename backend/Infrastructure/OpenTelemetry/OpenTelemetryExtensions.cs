using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("OmCleanArch"))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:18889"); // OTLP gRPC endpoint
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
                    options.Endpoint = new Uri("http://localhost:18889"); // OTLP gRPC endpoint
                });
            });

        return services;
    }
}

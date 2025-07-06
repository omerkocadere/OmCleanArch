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
        var options = OpenTelemetryOptions.FromSerilogConfiguration(configuration);

        if (string.IsNullOrEmpty(options?.Endpoint))
        {
            throw new InvalidOperationException("OTLP endpoint is not configured in Serilog sink.");
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                if (!string.IsNullOrEmpty(options.ServiceName))
                {
                    resource.AddService(options.ServiceName);
                }
                else
                {
                    resource.AddService(DiagnosticsConfig.ServiceName);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(DiagnosticsConfig.Meter.Name)
                    .AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri(options.Endpoint);
                        // Optionally set protocol if needed
                        // exporterOptions.Protocol = ...
                    });
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri(options.Endpoint);
                        // Optionally set protocol if needed
                        // exporterOptions.Protocol = ...
                    });
            });

        return services;
    }
}

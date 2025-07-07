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

        if (options?.ServiceName == null)
        {
            throw new InvalidOperationException("OTLP service name is not configured in config");
        }

        var diagnosticsServiceName = options.ServiceName;

        DiagnosticsConfig.Initialize(diagnosticsServiceName);

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(options.ServiceName);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(DiagnosticsConfig.Meter.Name)
                    .AddOtlpExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    // .AddNpgsql()
                    .AddOtlpExporter();
            });

        return services;
    }
}

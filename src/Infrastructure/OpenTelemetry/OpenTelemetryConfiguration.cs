using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class OpenTelemetryConfiguration
{
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, IConfiguration configuration)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(cfg =>
            {
                cfg.AddService(builder.Environment.ApplicationName);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsqlInstrumentation()
                    .AddMeter(DiagnosticsConfig.Meter.Name);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddProcessor(new HangfireTaggerProcessor());
            });

        builder.AddOpenTelemetryExporters(configuration);

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder, IConfiguration configuration)
        where TBuilder : IHostApplicationBuilder
    {
        var options = configuration.GetSection(OpenTelemetryOptions.SectionName).Get<OpenTelemetryOptions>();

        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var useOtlpExporter = !string.IsNullOrWhiteSpace(otlpEndpoint);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
        else if (!string.IsNullOrWhiteSpace(options?.Endpoint))
        {
            var protocol = options.Protocol switch
            {
                OtlpProtocol.Grpc => OtlpExportProtocol.Grpc,
                OtlpProtocol.HttpProtobuf => OtlpExportProtocol.HttpProtobuf,
                _ => OtlpExportProtocol.Grpc,
            };
            builder.Services.AddOpenTelemetry().UseOtlpExporter(protocol, new Uri(options.Endpoint));
        }
        else
        {
            throw new InvalidOperationException("No OTLP endpoint configuration found.");
        }

        return builder;
    }
}

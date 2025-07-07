using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class OpenTelemetryLoggingBuilderExtensions
{
    public static ILoggingBuilder AddOpenTelemetryLogging(
        this ILoggingBuilder builder,
        IConfiguration configuration
    )
    {
        // var options = configuration
        //     .GetSection(OpenTelemetryOptions.SectionName)
        //     .Get<OpenTelemetryOptions>();

        // if (options == null || string.IsNullOrEmpty(options.Endpoint))
        // {
        //     throw new InvalidOperationException(
        //         "OpenTelemetry Endpoint is missing or invalid in configuration."
        //     );
        // }
        builder.AddOpenTelemetry(logging =>
        {
            logging.AddOtlpExporter();
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        return builder;
    }
}

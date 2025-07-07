using Microsoft.Extensions.Configuration;

namespace CleanArch.Infrastructure.OpenTelemetry;

public class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";
    public required string Endpoint { get; set; }
    public required string Protocol { get; set; } = "grpc";
    public required string ServiceName { get; set; } = "UnknownService";
    public string? Headers { get; set; }

    // Extracts OpenTelemetry sink args from Serilog config
    public static OpenTelemetryOptions FromSerilogConfiguration(IConfiguration configuration)
    {
        var writeToSection = configuration.GetSection("Serilog:WriteTo");
        foreach (var section in writeToSection.GetChildren())
        {
            if (section["Name"] == "OpenTelemetry")
            {
                var args = section.GetSection("Args");
                if (!args.Exists())
                {
                    throw new InvalidOperationException(
                        "OpenTelemetry Args section is missing in Serilog configuration."
                    );
                }

                var endpoint = args["endpoint"];
                var protocol = args["protocol"];

                if (string.IsNullOrEmpty(endpoint))
                {
                    throw new InvalidOperationException(
                        "OpenTelemetry endpoint is missing in Serilog configuration."
                    );
                }

                if (string.IsNullOrEmpty(protocol))
                {
                    throw new InvalidOperationException(
                        "OpenTelemetry protocol is missing in Serilog configuration."
                    );
                }

                var options = new OpenTelemetryOptions
                {
                    Endpoint = endpoint,
                    Protocol = protocol,
                    ServiceName = "UnknownService", // Default fallback
                };

                var resourceAttributesSection = args.GetSection("resourceAttributes");
                if (resourceAttributesSection.Exists())
                {
                    var serviceName = resourceAttributesSection["service.name"];
                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        options.ServiceName = serviceName;
                    }
                }

                return options;
            }
        }

        throw new InvalidOperationException("OpenTelemetry is not configured in Serilog sink.");
    }
}

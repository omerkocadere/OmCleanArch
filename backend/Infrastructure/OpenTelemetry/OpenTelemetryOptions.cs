using Microsoft.Extensions.Configuration;

namespace CleanArch.Infrastructure.OpenTelemetry;

public class OpenTelemetryOptions
{
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public string? ServiceName { get; set; }

    // Extracts OpenTelemetry sink args from Serilog config
    public static OpenTelemetryOptions? FromSerilogConfiguration(IConfiguration configuration)
    {
        var writeToSection = configuration.GetSection("Serilog:WriteTo");
        foreach (var section in writeToSection.GetChildren())
        {
            if (section["Name"] == "OpenTelemetry")
            {
                var args = section.GetSection("Args");
                var options = new OpenTelemetryOptions
                {
                    Endpoint = args["endpoint"],
                    Protocol = args["protocol"],
                };
                var resourceAttributesSection = args.GetSection("resourceAttributes");
                if (resourceAttributesSection.Exists())
                {
                    options.ServiceName = resourceAttributesSection["service.name"];
                }
                return options;
            }
        }
        return null;
    }
}

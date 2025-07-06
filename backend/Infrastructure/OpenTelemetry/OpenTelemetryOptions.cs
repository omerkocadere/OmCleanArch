namespace CleanArch.Infrastructure.OpenTelemetry;

public class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string? OtlpEndpoint { get; set; }
    // Add other OpenTelemetry-related options here as needed
}

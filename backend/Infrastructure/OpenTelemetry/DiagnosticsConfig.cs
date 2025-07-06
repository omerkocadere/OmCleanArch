using System.Diagnostics.Metrics;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class DiagnosticsConfig
{
    public const string ServiceName = "OmCleanArch";

    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<int> CreatedUserCounter = Meter.CreateCounter<int>(
        "createduser.count"
    );
}

using System.Diagnostics.Metrics;

namespace CleanArch.Infrastructure.OpenTelemetry;

public static class DiagnosticsConfig
{
    private static Meter? _meter;
    private static Counter<int>? _createdUserCounter;

    public static Meter Meter =>
        _meter ?? throw new InvalidOperationException("DiagnosticsConfig is not initialized");
    public static Counter<int> CreatedUserCounter =>
        _createdUserCounter
        ?? throw new InvalidOperationException("DiagnosticsConfig is not initialized");

    public static void Initialize(string serviceName)
    {
        _meter = new Meter(serviceName);
        _createdUserCounter = _meter.CreateCounter<int>("createduser.count");
    }
}

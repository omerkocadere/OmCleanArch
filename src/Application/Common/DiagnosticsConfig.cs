using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CleanArch.Application.Common;

public static class DiagnosticsConfig
{
    private static Meter? _meter;
    private static Counter<int>? _createdUserCounter;
    public static readonly ActivitySource ActivitySource = new("CleanArch.Application.Common");

    private static void EnsureInitialized()
    {
        if (_meter == null || _createdUserCounter == null)
        {
            _meter = new Meter("DefaultService");
            _createdUserCounter = _meter.CreateCounter<int>("createduser.count");
        }
    }

    public static Meter Meter
    {
        get
        {
            EnsureInitialized();
            return _meter!;
        }
    }
    public static Counter<int> CreatedUserCounter
    {
        get
        {
            EnsureInitialized();
            return _createdUserCounter!;
        }
    }

    public static void Initialize(string serviceName)
    {
        _meter = new Meter(serviceName);
        _createdUserCounter = _meter.CreateCounter<int>("createduser.count");
    }
}

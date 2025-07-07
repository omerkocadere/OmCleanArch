namespace CleanArch.Infrastructure.OpenTelemetry;

public enum OtlpProtocol
{
    Grpc,
    HttpProtobuf,
}

public class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";
    public required string Endpoint { get; set; }
    public OtlpProtocol Protocol { get; set; } = OtlpProtocol.Grpc;
    public required string ServiceName { get; set; } = "UnknownService";
    public string? Headers { get; set; }
}

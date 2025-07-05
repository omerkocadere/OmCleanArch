namespace CleanArch.Infrastructure.Data.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccuredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTime? ProcessingStartedAt { get; set; }
}

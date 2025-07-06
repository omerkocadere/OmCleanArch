namespace CleanArch.Infrastructure.BackgroundJobs.Outbox;

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
    public DateTimeOffset OccuredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public int RetryCount { get; set; }
}

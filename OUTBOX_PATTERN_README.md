# Outbox Pattern Implementation with Hangfire

## Overview

This implementation provides a robust outbox pattern using Hangfire for processing domain events asynchronously. The outbox pattern ensures reliable event delivery by storing domain events as outbox messages in the same database transaction, then processing them asynchronously via background jobs.

## Architecture Components

### 1. OutboxMessage Entity

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } // Domain event type name
    public string Content { get; set; } // Serialized JSON content
    public DateTime OccuredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; } // NULL = unprocessed
    public string? Error { get; set; } // Error details if processing failed
}
```

### 2. Domain Event Interceptor

`ConvertDomainEventsToOutputMessagesInterceptor` automatically converts domain events to outbox messages during `SaveChanges`, ensuring atomicity.

### 3. Background Job Processing

- `ProcessOutboxMessagesJob`: Processes unprocessed outbox messages
- `HangfireBackgroundJobService`: Manages job scheduling
- `IBackgroundJobService`: Abstraction for background job operations

## Key Features

### ✅ Reliability

- **Atomic Operations**: Domain events are stored as outbox messages in the same transaction
- **At-Least-Once Delivery**: Failed messages are retried automatically
- **Error Tracking**: Failed processing attempts are logged with error details

### ✅ Performance

- **Batch Processing**: Processes up to 20 messages per batch
- **Configurable Frequency**: Runs every minute (easily configurable)
- **Non-blocking**: Background processing doesn't affect main application performance

### ✅ Monitoring

- **Hangfire Dashboard**: Available at `/hangfire` in development
- **Comprehensive Logging**: Full traceability of message processing
- **Status Tracking**: Clear distinction between processed and unprocessed messages

## Configuration

### Database Setup

Hangfire uses the same SQLite database as the main application for simplicity. In production, consider using a separate database.

### Recurring Job Schedule

```csharp
// Every minute - customize as needed
RecurringJob.AddOrUpdate<ProcessOutboxMessagesJob>(
    "process-outbox-messages",
    job => job.Execute(CancellationToken.None),
    "* * * * *" // Cron expression
);
```

## Usage Examples

### 1. Triggering Domain Events

```csharp
var user = new User { /* properties */ };
user.AddDomainEvent(new UserRegisteredDomainEvent(userId));
await context.SaveChangesAsync(); // Outbox message automatically created
```

### 2. Event Handlers

```csharp
public class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // Business logic: send emails, create profiles, etc.
        return Task.CompletedTask;
    }
}
```

### 3. Manual Job Triggering

```csharp
[HttpPost("process-outbox")]
public IActionResult ProcessOutbox([FromServices] IBackgroundJobService jobService)
{
    jobService.EnqueueOutboxProcessing();
    return Ok();
}
```

## Testing

### Test Endpoint

A test endpoint is available at `POST /api/test/trigger-domain-event` that:

1. Creates a new user
2. Triggers a `UserRegisteredDomainEvent`
3. Saves to database (creates outbox message)
4. Returns confirmation

### Verification Steps

1. Call the test endpoint
2. Check the outbox messages in the database
3. Wait for background job to process (< 1 minute)
4. Verify `ProcessedOnUtc` is set and no errors
5. Check application logs for event handler execution

## Monitoring & Troubleshooting

### Hangfire Dashboard

- Access: `http://localhost:7700/hangfire` (development only)
- View: Job status, execution history, failures
- Actions: Manual job triggering, retry failed jobs

### Database Queries

```sql
-- Check unprocessed messages
SELECT * FROM OutboxMessage WHERE ProcessedOnUtc IS NULL;

-- Check failed messages
SELECT * FROM OutboxMessage WHERE Error IS NOT NULL;

-- Processing statistics
SELECT
    COUNT(*) as Total,
    COUNT(ProcessedOnUtc) as Processed,
    COUNT(Error) as Failed
FROM OutboxMessage;
```

### Common Issues & Solutions

1. **Messages not processing**: Check Hangfire server status in logs
2. **Deserialization errors**: Ensure domain event types are available in assembly
3. **Performance issues**: Adjust batch size and frequency in job configuration

## Production Considerations

### 1. Storage Optimization

- Consider separate Hangfire database
- Implement message cleanup for old processed messages
- Monitor database growth

### 2. Scaling

- Increase worker count for higher throughput
- Use multiple application instances (Hangfire handles distributed processing)
- Consider partitioning strategies for high-volume scenarios

### 3. Security

- Secure Hangfire dashboard in production
- Implement proper authentication/authorization
- Monitor for sensitive data in error logs

### 4. Monitoring

- Set up alerts for failed message processing
- Monitor processing latency and throughput
- Implement health checks for background job processing

## Benefits Achieved

✅ **Reliability**: No lost domain events due to atomic storage  
✅ **Scalability**: Asynchronous processing doesn't block main operations  
✅ **Maintainability**: Clean separation of concerns with clear interfaces  
✅ **Observability**: Full visibility into event processing pipeline  
✅ **Flexibility**: Easy to add new event handlers and modify processing logic

The implementation follows SOLID principles and provides a robust foundation for event-driven architecture in Clean Architecture applications.

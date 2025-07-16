using System.Text.Json;
using CleanArch.Domain.Common;
using CleanArch.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace CleanArch.Infrastructure.BackgroundJobs.Outbox;

public class ProcessOutboxMessagesJob(
    ApplicationDbContext context,
    IPublisher publisher,
    ILogger<ProcessOutboxMessagesJob> logger
)
{
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing outbox messages started");

        var outboxMessages = await context
            .OutboxMessages.Where(x => x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.OccuredOnUtc)
            .Take(20) // Process in batches
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} outbox messages to process", outboxMessages.Count);

        // Lock messages by setting status to Processing
        foreach (var message in outboxMessages)
        {
            message.Status = OutboxMessageStatus.Processing;
            message.ProcessingStartedAt = DateTime.UtcNow;
        }

        if (outboxMessages.Count != 0)
        {
            await context.SaveChangesAsync(cancellationToken); // Lock messages immediately
        }

        foreach (var outboxMessage in outboxMessages)
        {
            BaseEvent? domainEvent = null;

            try
            {
                domainEvent = DeserializeDomainEvent(outboxMessage.Type, outboxMessage.Content);
            }
            catch (Exception ex)
            {
                HandleFailedMessage(outboxMessage, ex.Message, logger);
            }

            if (domainEvent is null)
                return;

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            var result = await policy.ExecuteAndCaptureAsync(() => publisher.Publish(domainEvent, cancellationToken));

            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            outboxMessage.Status = OutboxMessageStatus.Failed;
            outboxMessage.Error = result.FinalException?.Message;
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Processing outbox messages completed");
    }

    private static BaseEvent? DeserializeDomainEvent(string type, string content)
    {
        // Get the domain event type from the type name
        var domainEventType = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(t => t.Name == type && typeof(BaseEvent).IsAssignableFrom(t));

        if (domainEventType == null)
            return null;

        var domainEvent = JsonSerializer.Deserialize(content, domainEventType);
        return domainEvent as BaseEvent;
    }

    private static void HandleFailedMessage(
        OutboxMessage outboxMessage,
        string errorMessage,
        ILogger<ProcessOutboxMessagesJob> logger
    )
    {
        const int maxRetryAttempts = 3;

        outboxMessage.RetryCount++;
        outboxMessage.Error = errorMessage;

        if (outboxMessage.RetryCount >= maxRetryAttempts)
        {
            outboxMessage.Status = OutboxMessageStatus.Failed; // Permanent failure
        }
        else
        {
            outboxMessage.Status = OutboxMessageStatus.Pending; // Retry
        }

        logger.LogError(
            "Failed to process outbox message {MessageId} of type {MessageType}. Error: {ErrorMessage}",
            outboxMessage.Id,
            outboxMessage.Type,
            errorMessage
        );
    }
}

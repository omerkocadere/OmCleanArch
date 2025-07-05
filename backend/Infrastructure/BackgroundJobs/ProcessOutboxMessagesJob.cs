using System.Text.Json;
using CleanArch.Domain.Common;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.Data.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.BackgroundJobs;

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
            try
            {
                var domainEvent = DeserializeDomainEvent(outboxMessage.Type, outboxMessage.Content);

                if (domainEvent is not null)
                {
                    await publisher.Publish(domainEvent, cancellationToken);

                    outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
                    outboxMessage.Status = OutboxMessageStatus.Completed;
                    outboxMessage.Error = null;

                    logger.LogInformation(
                        "Successfully processed outbox message {MessageId} of type {MessageType}",
                        outboxMessage.Id,
                        outboxMessage.Type
                    );
                }
                else
                {
                    outboxMessage.Error =
                        $"Failed to deserialize domain event of type {outboxMessage.Type}";

                    logger.LogWarning(
                        "Failed to deserialize outbox message {MessageId} of type {MessageType}",
                        outboxMessage.Id,
                        outboxMessage.Type
                    );

                    // Throw exception to trigger Hangfire retry
                    throw new InvalidOperationException(
                        $"Failed to deserialize domain event of type {outboxMessage.Type}"
                    );
                }
            }
            catch (Exception ex)
            {
                outboxMessage.Error = ex.Message;

                logger.LogError(
                    ex,
                    "Error processing outbox message {MessageId} of type {MessageType}",
                    outboxMessage.Id,
                    outboxMessage.Type
                );

                // Don't handle the exception - let Hangfire retry
                throw;
            }
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
}

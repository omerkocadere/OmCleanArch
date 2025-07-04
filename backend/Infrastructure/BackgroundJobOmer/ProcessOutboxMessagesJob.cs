using System.Text.Json;
using CleanArch.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.BackgroundJobOmer;

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
            .OutboxMessages.Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccuredOnUtc)
            .Take(20) // Process in batches
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} outbox messages to process", outboxMessages.Count);

        foreach (var outboxMessage in outboxMessages)
        {
            try
            {
                var domainEvent = DeserializeDomainEvent(outboxMessage.Type, outboxMessage.Content);

                if (domainEvent is not null)
                {
                    await publisher.Publish(domainEvent, cancellationToken);

                    outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
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
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Processing outbox messages completed");
    }

    private static INotification? DeserializeDomainEvent(string type, string content)
    {
        // Get the domain event type from the type name
        var domainEventType = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(t => t.Name == type && typeof(INotification).IsAssignableFrom(t));

        if (domainEventType == null)
            return null;

        var domainEvent = JsonSerializer.Deserialize(content, domainEventType);
        return domainEvent as INotification;
    }
}

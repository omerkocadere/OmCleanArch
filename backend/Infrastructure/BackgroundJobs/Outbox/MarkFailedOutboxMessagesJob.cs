using CleanArch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.BackgroundJobs.Outbox;

public class MarkFailedOutboxMessagesJob(
    ApplicationDbContext context,
    ILogger<MarkFailedOutboxMessagesJob> logger
)
{
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Marking old processing outbox messages as failed started");

        // Mark messages that have been processing for more than 1 hour as failed
        var cutoffTime = DateTime.UtcNow.AddHours(-1);

        var stuckMessages = await context
            .OutboxMessages.Where(x =>
                x.Status == OutboxMessageStatus.Processing && x.ProcessingStartedAt < cutoffTime
            )
            .ToListAsync(cancellationToken);

        if (stuckMessages.Any())
        {
            foreach (var message in stuckMessages)
            {
                message.Status = OutboxMessageStatus.Failed;
                message.Error =
                    message.Error
                    ?? "Job exceeded maximum processing time and was marked as failed";

                logger.LogWarning(
                    "Marked outbox message {MessageId} of type {MessageType} as failed after timeout",
                    message.Id,
                    message.Type
                );
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Marked {Count} stuck outbox messages as failed",
                stuckMessages.Count
            );
        }

        logger.LogInformation("Marking old processing outbox messages as failed completed");
    }
}

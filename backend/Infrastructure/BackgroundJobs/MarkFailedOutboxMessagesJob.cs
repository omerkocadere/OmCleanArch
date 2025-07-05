using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.Data.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.BackgroundJobs;

public class MarkFailedOutboxMessagesJob(
    ApplicationDbContext context,
    ILogger<MarkFailedOutboxMessagesJob> logger
)
{
    public async Task Execute(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Marking old processing outbox messages as failed started");

        // Mark messages that have been processing for more than 20 minutes as failed
        var cutoffTime = DateTime.UtcNow.AddMinutes(-20);

        var stuckMessages = await context
            .OutboxMessages.Where(x =>
                x.Status == OutboxMessageStatus.Processing && x.ProcessingStartedAt < cutoffTime
            )
            .ToListAsync(cancellationToken);

        if (stuckMessages.Count != 0)
        {
            foreach (var message in stuckMessages)
            {
                message.Status = OutboxMessageStatus.Failed;
                message.Error ??= "Job exceeded maximum processing time and was marked as failed";

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

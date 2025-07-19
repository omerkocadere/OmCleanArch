using CleanArch.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.BackgroundJobs;

/// <summary>
/// No-operation implementation of IBackgroundJobService.
/// Used when background jobs are disabled in configuration.
/// All methods are no-ops and log that background jobs are disabled.
/// </summary>
public class NoOpBackgroundJobService(ILogger<NoOpBackgroundJobService> logger) : IBackgroundJobService
{
    public void EnqueueOutboxProcessing()
    {
        logger.LogDebug("Background jobs are disabled. Skipping outbox processing enqueue.");
    }

    public void ScheduleRecurringOutboxProcessing()
    {
        logger.LogDebug("Background jobs are disabled. Skipping recurring outbox processing schedule.");
    }

    public void ScheduleRecurringFailedMessageCleanup()
    {
        logger.LogDebug("Background jobs are disabled. Skipping recurring failed message cleanup schedule.");
    }
}

using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using Hangfire;

namespace CleanArch.Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobService(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager
) : IBackgroundJobService
{
    public void EnqueueOutboxProcessing()
    {
        backgroundJobClient.Enqueue<ProcessOutboxMessagesJob>(job => job.Execute(CancellationToken.None));
    }

    public void ScheduleRecurringOutboxProcessing()
    {
        recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
            "process-outbox-messages",
            job => job.Execute(CancellationToken.None),
            "* * * * *" // Run every minute (Cron expression)
        );
    }

    public void ScheduleRecurringFailedMessageCleanup()
    {
        recurringJobManager.AddOrUpdate<MarkFailedOutboxMessagesJob>(
            "mark-failed-outbox-messages",
            job => job.Execute(CancellationToken.None),
            "*/10 * * * *" // Run every 10 minutes
        );
    }
}

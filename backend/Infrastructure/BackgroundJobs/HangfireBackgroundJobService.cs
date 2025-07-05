using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using Hangfire;

namespace CleanArch.Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public void EnqueueOutboxProcessing()
    {
        BackgroundJob.Enqueue<ProcessOutboxMessagesJob>(job => job.Execute(CancellationToken.None));
    }

    public void ScheduleRecurringOutboxProcessing()
    {
        RecurringJob.AddOrUpdate<ProcessOutboxMessagesJob>(
            "process-outbox-messages",
            job => job.Execute(CancellationToken.None),
            "* * * * *" // Run every minute (Cron expression)
        );
    }

    public void ScheduleRecurringFailedMessageCleanup()
    {
        RecurringJob.AddOrUpdate<MarkFailedOutboxMessagesJob>(
            "mark-failed-outbox-messages",
            job => job.Execute(CancellationToken.None),
            "0 */10 * * *" // Run every 10 minutes
        );
    }
}

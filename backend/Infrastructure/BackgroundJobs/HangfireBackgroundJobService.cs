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
}

namespace CleanArch.Infrastructure.BackgroundJobs;

public interface IBackgroundJobService
{
    void EnqueueOutboxProcessing();
    void ScheduleRecurringOutboxProcessing();
}

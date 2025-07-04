namespace CleanArch.Infrastructure.BackgroundJobOmer;

public interface IBackgroundJobService
{
    void EnqueueOutboxProcessing();
    void ScheduleRecurringOutboxProcessing();
}

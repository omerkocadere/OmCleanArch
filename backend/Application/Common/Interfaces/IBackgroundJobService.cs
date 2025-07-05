namespace CleanArch.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    void EnqueueOutboxProcessing();
    void ScheduleRecurringOutboxProcessing();
    void ScheduleRecurringFailedMessageCleanup();
}

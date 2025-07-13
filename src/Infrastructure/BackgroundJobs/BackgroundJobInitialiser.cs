using CleanArch.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure.BackgroundJobs;

public static class BackgroundJobInitialiser
{
    public static void InitializeBackgroundJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
        backgroundJobService.ScheduleRecurringOutboxProcessing();
        backgroundJobService.ScheduleRecurringFailedMessageCleanup();
    }
}

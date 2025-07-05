using CleanArch.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure.BackgroundJobs;

public static class BackgroundJobExtensions
{
    public static WebApplication ConfigureBackgroundJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var backgroundJobService =
            scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
        backgroundJobService.ScheduleRecurringOutboxProcessing();
        backgroundJobService.ScheduleRecurringFailedMessageCleanup();

        return app;
    }
}

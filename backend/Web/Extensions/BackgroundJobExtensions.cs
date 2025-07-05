using CleanArch.Infrastructure.BackgroundJobs;

namespace CleanArch.Web.Extensions;

public static class BackgroundJobExtensions
{
    public static WebApplication ConfigureBackgroundJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var backgroundJobService =
            scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
        backgroundJobService.ScheduleRecurringOutboxProcessing();

        return app;
    }
}

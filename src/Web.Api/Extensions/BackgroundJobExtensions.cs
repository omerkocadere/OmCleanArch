using CleanArch.Infrastructure.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanArch.Web.Api.Extensions;

/// <summary>
/// Extension methods for configuring Hangfire dashboard and background job initialization
/// based on configuration settings.
/// </summary>
public static class BackgroundJobExtensions
{
    /// <summary>
    /// Conditionally configures Hangfire dashboard based on BackgroundJobs configuration.
    /// Only enables dashboard when both BackgroundJobs.Enabled and BackgroundJobs.Dashboard.Enabled are true.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboardConditionally(this WebApplication app)
    {
        var backgroundJobOptions = app.Services.GetRequiredService<IOptions<BackgroundJobOptions>>().Value;

        // Only configure dashboard if background jobs and dashboard are both enabled
        if (!backgroundJobOptions.Enabled || !backgroundJobOptions.Dashboard.Enabled)
        {
            return app;
        }

        var dashboardOptions = new Hangfire.DashboardOptions();

        // Configure authorization based on settings
        if (!backgroundJobOptions.Dashboard.RequireAuthorization)
        {
            dashboardOptions.Authorization = [new NoAuthorizationFilter()];
        }

        app.UseHangfireDashboard(backgroundJobOptions.Dashboard.Path, dashboardOptions);

        return app;
    }

    /// <summary>
    /// Conditionally initializes background jobs based on BackgroundJobs configuration.
    /// Only initializes when BackgroundJobs.Enabled and BackgroundJobs.AutoInitialize are both true.
    /// </summary>
    public static void InitializeBackgroundJobsConditionally(this WebApplication app)
    {
        var backgroundJobOptions = app.Services.GetRequiredService<IOptions<BackgroundJobOptions>>().Value;

        // Only initialize if background jobs are enabled and auto-initialization is enabled
        if (!backgroundJobOptions.Enabled || !backgroundJobOptions.AutoInitialize)
        {
            return;
        }

        app.InitializeBackgroundJobs();
    }
}

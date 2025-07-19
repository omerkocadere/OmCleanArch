using System.ComponentModel.DataAnnotations;

namespace CleanArch.Infrastructure.BackgroundJobs;

/// <summary>
/// Configuration options for background job processing with Hangfire.
/// Controls whether background jobs and dashboard are enabled.
/// </summary>
public class BackgroundJobOptions
{
    public const string SectionName = "BackgroundJobs";

    /// <summary>
    /// Enables or disables background job processing.
    /// When false, no background jobs will be registered or processed.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Configuration for Hangfire dashboard.
    /// </summary>
    public HangfireDashboardOptions Dashboard { get; set; } = new();

    /// <summary>
    /// Whether to automatically initialize recurring background jobs on application startup.
    /// Only applies when Enabled is true.
    /// </summary>
    public bool AutoInitialize { get; set; } = true;
}

/// <summary>
/// Configuration options for Hangfire dashboard.
/// </summary>
public class HangfireDashboardOptions
{
    /// <summary>
    /// Enables or disables the Hangfire dashboard.
    /// When false, dashboard will not be accessible even if background jobs are enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// The path where the dashboard will be accessible.
    /// Default is "/hangfire".
    /// </summary>
    public string Path { get; set; } = "/hangfire";

    /// <summary>
    /// Whether to require authorization for dashboard access.
    /// When false, uses NoAuthorizationFilter (only for development).
    /// When true, requires proper authentication (recommended for production).
    /// </summary>
    public bool RequireAuthorization { get; set; } = true;
}

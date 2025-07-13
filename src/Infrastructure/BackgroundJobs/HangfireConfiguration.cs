using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using CleanArch.Infrastructure.Data.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.BackgroundJobs;

public static class HangfireConfiguration
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(
            (sp, configuration) =>
            {
                var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                var connectionString = databaseOptions.Provider switch
                {
                    DbProvider.Sqlite => databaseOptions.SqliteConnectionString,
                    DbProvider.Postgres => databaseOptions.PostgresConnectionString,
                    _ => throw new InvalidOperationException(
                        $"Unsupported database provider: {databaseOptions.Provider}"
                    ),
                };

                switch (databaseOptions.Provider)
                {
                    case DbProvider.Sqlite:
                        var hangfireConn = GetHangfireSqliteConnectionString(connectionString!);
                        configuration.UseSQLiteStorage(hangfireConn);
                        break;
                    case DbProvider.Postgres:
                        configuration.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Unsupported database provider: {databaseOptions.Provider}"
                        );
                }
            }
        );

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        GlobalJobFilters.Filters.Add(
            new AutomaticRetryAttribute
            {
                Attempts = 5, // Retry 5 times
                DelaysInSeconds = [10, 30, 60, 300, 600], // 10s, 30s, 1m, 5m, 10m
            }
        );

        // Register background job services
        services.AddScoped<ProcessOutboxMessagesJob>();
        services.AddScoped<MarkFailedOutboxMessagesJob>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }

    private static string GetHangfireSqliteConnectionString(string original)
    {
        // Extracts the base name from Data Source and appends 'Hangfire'
        var parts = original.Split(';');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                var fileName = trimmed["Data Source=".Length..].Trim();
                var name = Path.GetFileNameWithoutExtension(fileName);
                return name + "Hangfire";
            }
        }
        throw new InvalidOperationException("No Data Source found in connection string.");
    }
}

/// <summary>
/// Authorization filter that allows unrestricted access to Hangfire dashboard.
/// Use only in development/docker environments, not recommended for production.
/// </summary>
public class NoAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context) => true;
}

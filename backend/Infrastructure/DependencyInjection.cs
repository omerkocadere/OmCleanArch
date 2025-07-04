using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.BackgroundJobOmer;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.Data.Interceptors;
using CleanArch.Infrastructure.Data.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IHostEnvironment env
    )
    {
        return services
            .AddServices()
            .AddDatabase(env)
            .AddBackgroundJobs()
            .AddAuthenticationInternal();
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IHostEnvironment env
    )
    {
        services.ConfigureOptions<DatabaseOptionsSetup>();

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogInformation(
                    "Selected database provider: {Provider}",
                    databaseOptions.Provider
                );

                switch (databaseOptions.Provider)
                {
                    case DbProvider.Sqlite:
                        ValidateConnectionString(
                            databaseOptions.SqliteConnectionString,
                            DbProvider.Sqlite
                        );
                        options.UseSqlite(databaseOptions.SqliteConnectionString);
                        break;
                    case DbProvider.Postgres:
                        ValidateConnectionString(
                            databaseOptions.PostgresConnectionString,
                            DbProvider.Postgres
                        );
                        options.UseNpgsql(databaseOptions.PostgresConnectionString);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Unsupported database provider: {databaseOptions.Provider}"
                        );
                }

                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

                if (env.IsDevelopment())
                {
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                }
            }
        );

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        // services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<
            ISaveChangesInterceptor,
            ConvertDomainEventsToOutputMessagesInterceptor
        >();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
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
                        configuration.UseSQLiteStorage(connectionString);
                        break;
                    case DbProvider.Postgres:
                        configuration.UsePostgreSqlStorage(options =>
                            options.UseNpgsqlConnection(connectionString)
                        );
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

        // Register background job services
        services.AddScoped<ProcessOutboxMessagesJob>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        return services;
    }

    private static void ValidateConnectionString(string? connectionString, DbProvider provider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Database connection string for '{provider}' is missing in configuration."
            );
    }
}

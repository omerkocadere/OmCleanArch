using CleanArch.Application.Common.Interfaces;
using CleanArch.Infrastructure.Data.Interceptors;
using CleanArch.Infrastructure.Data.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Data;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IHostEnvironment env,
        IConfiguration configuration
    )
    {
        services.ConfigureOptions<DatabaseOptionsSetup>();

        // Log database provider selection once during startup
        var dbOptions = configuration.GetSection(DatabaseOptionsSetup.ConfigurationSectionName).Get<DatabaseOptions>();
        if (dbOptions != null)
        {
            // Create a temporary logger for startup logging
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger(nameof(DatabaseConfiguration));
            logger.LogInformation("Selected database provider: {Provider}", dbOptions.Provider);
        }

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                switch (databaseOptions.Provider)
                {
                    case DbProvider.Sqlite:
                        ValidateConnectionString(databaseOptions.SqliteConnectionString, DbProvider.Sqlite);
                        options.UseSqlite(databaseOptions.SqliteConnectionString);
                        break;
                    case DbProvider.Postgres:
                        ValidateConnectionString(databaseOptions.PostgresConnectionString, DbProvider.Postgres);
                        options.UseNpgsql(databaseOptions.PostgresConnectionString);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Unsupported database provider: {databaseOptions.Provider}"
                        );
                }

                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

                // Suppress EF Core warnings about global query filters on required relationships
                // This is the industry standard approach for Aggregate Root pattern
                options.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
                });

                if (env.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            }
        );

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        // services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, ConvertDomainEventsToOutputMessagesInterceptor>();
        AddConditionalHealthChecks(services, configuration);

        return services;
    }

    private static void AddConditionalHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptionsSetup.ConfigurationSectionName).Get<DatabaseOptions>();
        if (dbOptions?.Provider == DbProvider.Postgres)
        {
            ValidateConnectionString(dbOptions.PostgresConnectionString, DbProvider.Postgres);
            services.AddHealthChecks().AddNpgSql(dbOptions.PostgresConnectionString!);
        }
    }

    private static void ValidateConnectionString(string? connectionString, DbProvider provider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Database connection string for '{provider}' is missing in configuration."
            );
    }
}

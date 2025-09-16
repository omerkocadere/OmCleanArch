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
        // Configure DatabaseOptions directly from configuration
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Log database provider selection once during startup
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();
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
                        options.UseNpgsql(
                            databaseOptions.PostgresConnectionString,
                            npgsqlOptions =>
                            {
                                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                                npgsqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorCodesToAdd: null
                                );
                            }
                        );
                        break;
                    case DbProvider.SqlServer:
                        ValidateConnectionString(databaseOptions.SqlServerConnectionString, DbProvider.SqlServer);
                        options.UseSqlServer(databaseOptions.SqlServerConnectionString);
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

        /*
         * NOTE: DispatchDomainEventsInterceptor is intentionally commented out.
         *
         * REASON: Replaced with Outbox Pattern for better reliability and performance
         *
         * OLD APPROACH (DispatchDomainEventsInterceptor):
         * - Synchronous domain event processing during SaveChanges
         * - Direct MediatR.Publish() call within transaction
         * - Performance impact (blocks SaveChanges until all events processed)
         * - No retry mechanism for failed events
         * - Single point of failure
         *
         * NEW APPROACH (ConvertDomainEventsToOutputMessagesInterceptor + ProcessOutboxMessagesJob):
         * - Asynchronous domain event processing via Outbox Pattern
         * - Domain events → JSON serialized to OutboxMessage table (same transaction)
         * - Background job (ProcessOutboxMessagesJob) processes outbox messages separately
         * - Built-in retry mechanism for failed events
         * - Better performance (SaveChanges completes faster)
         * - Fault tolerance and reliability
         * - Transactional consistency guaranteed
         *
         * FLOW:
         * 1. Entity.AddDomainEvent() → Domain event added to entity
         * 2. SaveChanges() → ConvertDomainEventsToOutputMessagesInterceptor converts events to OutboxMessage
         * 3. ProcessOutboxMessagesJob (background) → Deserializes and publishes via MediatR
         * 4. Event handlers process the domain events asynchronously
         *
         * DO NOT uncomment unless you want to revert to synchronous processing!
         */
        AddConditionalHealthChecks(services, configuration);

        return services;
    }

    private static void AddConditionalHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();
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

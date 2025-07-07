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
        string? connectionString = configuration.GetConnectionString("Database");

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
                        options.UseNpgsql(connectionString);
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Unsupported database provider: {databaseOptions.Provider}"
                        );
                }

                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

                if (env.IsDevelopment() || env.IsEnvironment("Docker"))
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
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

        services.AddHealthChecks().AddNpgSql(configuration.GetConnectionString("Database")!);

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

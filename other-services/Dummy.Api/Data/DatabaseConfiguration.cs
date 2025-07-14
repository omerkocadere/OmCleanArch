using Microsoft.EntityFrameworkCore;

namespace Dummy.Api.Data;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IHostEnvironment env,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No connection string found for 'DefaultConnection'.");
        }

        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

                logger.LogInformation("Using connection string: {ConnectionString}", connectionString);
                options.UseNpgsql(connectionString);

                if (env.IsDevelopment() || env.IsEnvironment("Docker"))
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            }
        );

        services.AddHealthChecks().AddNpgSql(connectionString);

        return services;
    }
}

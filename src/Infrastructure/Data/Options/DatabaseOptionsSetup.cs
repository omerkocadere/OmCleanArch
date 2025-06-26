using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Data.Options;

public class DatabaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DatabaseOptions>
{
    private const string ConfigurationSectionName = "DatabaseSettings";
    private const string ConnectionStringName = "CleanArchDb";

    public void Configure(DatabaseOptions options)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Database connection string '{ConnectionStringName}' is missing in configuration."
            );
        options.ConnectionString = connectionString;
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}

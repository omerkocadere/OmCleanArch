using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArch.Infrastructure.Data.Options;

public class DatabaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DatabaseOptions>
{
    private const string ConfigurationSectionName = "DatabaseSettings";

    public void Configure(DatabaseOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}

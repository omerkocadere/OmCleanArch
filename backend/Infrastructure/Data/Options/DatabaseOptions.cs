namespace CleanArch.Infrastructure.Data.Options;

public class DatabaseOptions
{
    public string? SqliteConnectionString { get; set; }
    public string? PostgresConnectionString { get; set; }
    public DbProvider Provider { get; set; } = DbProvider.Sqlite;
    public int CommandTimeout { get; set; }
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}

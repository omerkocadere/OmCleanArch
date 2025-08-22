namespace CleanArch.Infrastructure.Data.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "DatabaseSettings";

    public string? SqliteConnectionString { get; set; }
    public string? PostgresConnectionString { get; set; }
    public DbProvider Provider { get; set; } = DbProvider.Sqlite;
    public int CommandTimeout { get; set; }
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
}

public enum DbProvider
{
    Sqlite,
    Postgres,
}

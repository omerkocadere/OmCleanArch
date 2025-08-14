namespace CleanArch.Infrastructure.Configuration;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public string Provider { get; set; } = "Memory";
    public int DefaultTimeoutMinutes { get; set; } = 30;
    public string RedisConnectionString { get; set; } = "localhost:6379";

    public TimeSpan DefaultTimeout => TimeSpan.FromMinutes(DefaultTimeoutMinutes);
}

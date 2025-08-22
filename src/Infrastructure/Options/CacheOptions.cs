namespace CleanArch.Infrastructure.Options;

public static class CacheProviders
{
    public const string Redis = "Redis";
    public const string Memory = "Memory";
}

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public string Provider { get; set; } = CacheProviders.Memory;
    public int DefaultTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Redis connection string. Required when Provider is Redis.
    /// </summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    public TimeSpan DefaultTimeout => TimeSpan.FromMinutes(DefaultTimeoutMinutes);
}

namespace CleanArch.Application.Common.Interfaces.Messaging;

/// <summary>
/// Marker interface for queries that should be cached.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// The cache key for this query.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Cache expiration time. If null, uses default expiration.
    /// </summary>
    TimeSpan? Expiration { get; }
}

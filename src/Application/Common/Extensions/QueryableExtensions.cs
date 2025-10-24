using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using CleanArch.Application.Common.Models;

namespace CleanArch.Application.Common.Extensions;

// Fluent extensions for IQueryable to simplify filtering, sorting, and pagination
public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate
    )
    {
        return condition ? query.Where(predicate) : query;
    }

    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, string? value, Expression<Func<T, bool>> predicate)
    {
        return !string.IsNullOrWhiteSpace(value) ? query.Where(predicate) : query;
    }

    public static IQueryable<T> WhereIf<T, TValue>(
        this IQueryable<T> query,
        TValue? value,
        Expression<Func<T, bool>> predicate
    )
        where TValue : struct
    {
        return value.HasValue ? query.Where(predicate) : query;
    }

    // Applies dynamic sorting using System.Linq.Dynamic.Core
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query;

        return DynamicQueryableExtensions.OrderBy(query, orderBy);
    }

    // Applies pagination with AsNoTracking for better performance
    public static Task<PaginatedList<TEntity>> PageByAsync<TEntity>(
        this IQueryable<TEntity> query,
        QueryParamsBase pagingParams
    )
        where TEntity : class
    {
        return PaginatedList<TEntity>.CreateAsync(
            query.AsNoTracking(),
            pagingParams.PageNumberValue,
            pagingParams.PageSizeValue
        );
    }

    // Projects entity queryable to DTO list with mapping and AsNoTracking
    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
        this IQueryable queryable,
        CancellationToken cancellationToken = default
    )
        where TDestination : class
    {
        return queryable.ProjectToType<TDestination>().AsNoTracking().ToListAsync(cancellationToken);
    }

    // Projects filtered/sorted entity queryable to paginated DTO list
    public static Task<PaginatedList<TDestination>> ProjectToPagedAsync<TSource, TDestination>(
        this IQueryable<TSource> queryable,
        QueryParamsBase pagingParams
    )
        where TSource : class
        where TDestination : class
    {
        var projectedQuery = queryable.ProjectToType<TDestination>().AsNoTracking();
        return PaginatedList<TDestination>.CreateAsync(
            projectedQuery,
            pagingParams.PageNumberValue,
            pagingParams.PageSizeValue
        );
    }

    // ==================== SOFT DELETE EXTENSIONS ====================

    /// <summary>
    /// Includes soft-deleted entities in the query results by ignoring global query filters.
    /// Use this in admin panels or audit trails where deleted records need to be visible.
    /// </summary>
    /// <example>
    /// var allOrders = await context.PurchaseOrders
    ///     .WithDeleted()
    ///     .ToListAsync();
    /// </example>
    public static IQueryable<T> WithDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters();
    }

    /// <summary>
    /// Returns only soft-deleted entities (IsDeleted = true).
    /// Useful for viewing recycle bin or deleted items list in admin panels.
    /// </summary>
    /// <example>
    /// var deletedOrders = await context.PurchaseOrders
    ///     .OnlyDeleted()
    ///     .ToListAsync();
    /// </example>
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters().Where(x => x.IsDeleted);
    }

    /// <summary>
    /// Explicitly returns only active (non-deleted) entities.
    /// This is the default behavior but can be used for clarity in code.
    /// </summary>
    /// <example>
    /// var activeOrders = await context.PurchaseOrders
    ///     .WithDeleted()
    ///     .OnlyActive() // Override and show only active
    ///     .ToListAsync();
    /// </example>
    public static IQueryable<T> OnlyActive<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable
    {
        return query.Where(x => !x.IsDeleted);
    }
}

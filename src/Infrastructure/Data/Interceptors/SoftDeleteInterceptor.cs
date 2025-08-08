using CleanArch.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.Infrastructure.Data.Interceptors;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        HandleSoftDeletes(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        HandleSoftDeletes(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void HandleSoftDeletes(DbContext? context)
    {
        if (context == null)
            return;

        var softDeleteEntries = context
            .ChangeTracker.Entries<ISoftDeletable>()
            .Where(entry => entry.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in softDeleteEntries)
        {
            // Convert hard delete to soft delete
            entry.State = EntityState.Modified;
            entry.Entity.SoftDelete();
        }
    }
}

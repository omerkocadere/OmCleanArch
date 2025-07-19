using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArch.Domain.Common;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArch.Infrastructure.Data.Interceptors;

public class ConvertDomainEventsToOutputMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        DbContext? context = eventData.Context;

        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var outboxMessages = context
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var domainEvents = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent =>
            {
                var content = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType(),
                    new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }
                );

                return new OutboxMessage
                {
                    Id = domainEvent.Id,
                    Type = domainEvent.GetType().Name,
                    Content = content,
                    OccuredOnUtc = DateTime.UtcNow,
                    Status = OutboxMessageStatus.Pending,
                };
            })
            .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

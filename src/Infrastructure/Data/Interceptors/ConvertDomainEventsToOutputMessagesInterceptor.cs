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

        // Step 1: Find entities with domain events
        var entitiesWithEvents = context
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        // Step 2: Extract and clear domain events
        var allDomainEvents = new List<BaseEvent>();
        foreach (var entity in entitiesWithEvents)
        {
            var domainEvents = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            allDomainEvents.AddRange(domainEvents);
        }

        // Step 3: Convert domain events to outbox messages
        var outboxMessages = new List<OutboxMessage>();
        foreach (var domainEvent in allDomainEvents)
        {
            // Serialize domain event to JSON content
            var content = JsonSerializer.Serialize(
                domainEvent,
                domainEvent.GetType(),
                new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }
            );

            // Create outbox message
            var outboxMessage = new OutboxMessage
            {
                Id = domainEvent.Id,
                Type = domainEvent.GetType().Name,
                Content = content, // JSON representation of the domain event
                OccuredOnUtc = DateTime.UtcNow,
                Status = OutboxMessageStatus.Pending,
            };

            outboxMessages.Add(outboxMessage);
        }

        // Step 4: Add outbox messages to context
        context.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

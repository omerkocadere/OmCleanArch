using CleanArch.Domain.Common;
using CleanArch.Infrastructure.BackgroundJobs.Outbox;
using CleanArch.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Idempotence;

public sealed class IdempotentDomainEventHandler<TDomainEvent>(
    INotificationHandler<TDomainEvent> decorated,
    ApplicationDbContext dbContext
) : INotificationHandler<TDomainEvent>
    where TDomainEvent : BaseEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated = decorated;

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        string consumer = _decorated.GetType().Name;

        if (
            await dbContext
                .Set<OutboxMessageConsumer>()
                .AnyAsync(
                    outboxMessageConsumer =>
                        outboxMessageConsumer.Id == notification.Id && outboxMessageConsumer.Name == consumer,
                    cancellationToken
                )
        )
        {
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        dbContext.Set<OutboxMessageConsumer>().Add(new OutboxMessageConsumer { Id = notification.Id, Name = consumer });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

using CleanArch.Domain.TodoItems;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.TodoItems.CreateTodoItem;

public class TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    : INotificationHandler<TodoItemCreatedEvent>
{
    public Task Handle(TodoItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("CleanArch Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}

using CleanArch.Domain.TodoItems;
using Microsoft.Extensions.Logging;

namespace CleanArch.Application.TodoItems.DeleteTodoItem;

public class TodoItemDeletedEventHandler(ILogger<TodoItemDeletedEventHandler> logger)
    : INotificationHandler<TodoItemDeletedEvent>
{
    public Task Handle(TodoItemDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("CleanArch Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}

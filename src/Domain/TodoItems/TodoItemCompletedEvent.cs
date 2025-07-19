namespace CleanArch.Domain.TodoItems;

public record TodoItemCompletedEvent(Guid Id, TodoItem Item) : BaseEvent(Id);

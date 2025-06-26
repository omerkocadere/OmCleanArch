namespace CleanArch.Domain.TodoItems;

public record TodoItemCompletedEvent(TodoItem Item) : BaseEvent;

namespace CleanArch.Domain.TodoItems;

public record TodoItemCreatedEvent(TodoItem Item) : BaseEvent;

namespace CleanArch.Domain.TodoItems;

public record TodoItemDeletedEvent(TodoItem Item) : BaseEvent;

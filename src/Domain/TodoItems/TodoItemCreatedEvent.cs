namespace CleanArch.Domain.TodoItems;

public record TodoItemCreatedEvent(Guid Id, TodoItem Item) : BaseEvent(Id);

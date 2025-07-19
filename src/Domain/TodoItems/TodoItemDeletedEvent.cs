namespace CleanArch.Domain.TodoItems;

public record TodoItemDeletedEvent(Guid Id, TodoItem Item) : BaseEvent(Id);

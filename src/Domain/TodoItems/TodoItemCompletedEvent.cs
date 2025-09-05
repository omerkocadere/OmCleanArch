namespace CleanArch.Domain.TodoItems;

public record TodoItemCompletedEvent(
    Guid Id,
    int TodoItemId,
    string Title,
    string? Note,
    string? Description,
    PriorityLevel Priority,
    Guid UserId,
    int ListId,
    List<string> Labels,
    DateTime? DueDate,
    DateTime? Reminder,
    DateTime CompletedAt
) : BaseEvent(Id)
{
    public static TodoItemCompletedEvent Create(TodoItem item) =>
        new(
            Guid.NewGuid(),
            item.Id,
            item.Title,
            item.Note,
            item.Description,
            item.Priority,
            item.UserId,
            item.ListId,
            item.Labels,
            item.DueDate,
            item.Reminder,
            DateTime.UtcNow
        );
}

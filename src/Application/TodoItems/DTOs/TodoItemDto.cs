namespace CleanArch.Application.TodoItems.DTOs;

public class TodoItemDto
{
    public int Id { get; init; }
    public int ListId { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public int Priority { get; init; }

    public string? Note { get; init; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
    public DateTime? CompletedAt { get; set; }
}

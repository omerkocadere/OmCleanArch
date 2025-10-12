using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.CreateTodoItem;

public record CreateTodoItemCommand() : IRequest<Result<TodoItemDto>>
{
    public int ListId { get; init; }
    public required string Title { get; init; }
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; }
    public Guid UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

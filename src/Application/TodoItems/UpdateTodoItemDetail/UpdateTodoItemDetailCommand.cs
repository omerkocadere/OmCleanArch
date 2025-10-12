using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.UpdateTodoItemDetail;

public record UpdateTodoItemDetailCommand() : IRequest<Result>
{
    public int Id { get; init; }
    public int ListId { get; init; }
    public required string Title { get; init; }
    public required Guid UserId { get; init; }
    public string? Note { get; init; }
    public string? Description { get; set; }
    public PriorityLevel Priority { get; init; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

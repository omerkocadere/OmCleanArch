using CleanArch.Domain.TodoLists;

namespace CleanArch.Domain.TodoItems;

public sealed class TodoItem : BaseAuditableEntity
{
    public int ListId { get; set; }
    public required string Title { get; set; }
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }

    public int UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
    public DateTime? CompletedAt { get; set; }

    private bool _done;
    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }

            _done = value;
        }
    }

    public TodoList List { get; set; } = null!;
}

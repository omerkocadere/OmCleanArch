using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.TodoItems;

public sealed class TodoItem : FullAuditableEntity<int>
{
    public int ListId { get; set; }
    public required string Title { get; set; }
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }

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
                AddDomainEvent(new TodoItemCompletedEvent(Guid.NewGuid(), this));
            }

            _done = value;
        }
    }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public TodoList List { get; set; } = null!;
}

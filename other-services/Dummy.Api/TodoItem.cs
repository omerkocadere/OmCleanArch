namespace Dummy.Api;

public sealed class TodoItem
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public required string Title { get; set; }
    public string? Note { get; set; }
    public DateTime? Reminder { get; set; }

    public int UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
    public DateTime? CompletedAt { get; set; }
    public bool Done { get; set; }
}

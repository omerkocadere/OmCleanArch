using CleanArch.Domain.TodoItems;

namespace CleanArch.Domain.TodoLists;

public sealed class TodoList : BaseAuditableEntity
{
    public string? Title { get; set; }
    public Colour Colour { get; set; } = Colour.White;
    public IList<TodoItem> Items { get; init; } = [];
    public int UserId { get; set; }
}

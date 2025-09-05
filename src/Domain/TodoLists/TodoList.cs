using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;

namespace CleanArch.Domain.TodoLists;

public sealed class TodoList : BaseAuditableEntity<int>
{
    public string? Title { get; set; }
    public Colour Colour { get; set; } = Colour.White;
    public IList<TodoItem> Items { get; init; } = [];
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}

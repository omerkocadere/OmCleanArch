namespace CleanArch.Infrastructure.Data.Seed;

public class UserCsv
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class TodoListCsv
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public int UserId { get; set; }
}

public class TodoItemCsv
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int Priority { get; set; }
    public string? Reminder { get; set; }
    public int UserId { get; set; }
    public string? Description { get; set; }
    public string? DueDate { get; set; }
    public string? Labels { get; set; }
    public string? CompletedAt { get; set; }
    public bool Done { get; set; }
}

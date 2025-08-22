using CleanArch.Application.TodoItems.DTOs;

namespace CleanArch.Application.TodoLists.GetTodos;

public record TodoListDto
{
    public int Id { get; init; }

    public required string Title { get; init; }

    public string? Colour { get; init; }

    public IReadOnlyCollection<TodoItemDto> Items { get; init; } = [];
}

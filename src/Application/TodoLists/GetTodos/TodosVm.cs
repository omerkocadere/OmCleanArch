using CleanArch.Application.Common.Models;

namespace CleanArch.Application.TodoLists.GetTodos;

public record TodosVm
{
    public IReadOnlyCollection<LookupDto> PriorityLevels { get; init; } = [];
    public IReadOnlyCollection<TodoListDto> Lists { get; init; } = [];
}

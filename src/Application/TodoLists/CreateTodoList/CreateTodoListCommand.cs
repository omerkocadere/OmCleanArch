using CleanArch.Application.TodoLists.GetTodos;

namespace CleanArch.Application.TodoLists.CreateTodoList;

public record CreateTodoListCommand(string Title, Guid UserId) : IRequest<Result<TodoListDto>>;

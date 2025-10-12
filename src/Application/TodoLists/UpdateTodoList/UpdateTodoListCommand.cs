namespace CleanArch.Application.TodoLists.UpdateTodoList;

public record UpdateTodoListCommand(int Id, string Title, string UserId) : IRequest<Result>;

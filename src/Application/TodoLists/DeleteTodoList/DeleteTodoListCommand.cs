namespace CleanArch.Application.TodoLists.DeleteTodoList;

public record DeleteTodoListCommand(int Id) : IRequest<Result>;

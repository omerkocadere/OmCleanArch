namespace CleanArch.Application.TodoItems.UpdateTodoItem;

public record UpdateTodoItemCommand(int Id, bool Done) : IRequest<Result>;

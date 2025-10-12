namespace CleanArch.Application.TodoItems.DeleteTodoItem;

public record DeleteTodoItemCommand(int Id) : IRequest<Result>;

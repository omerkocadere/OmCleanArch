namespace CleanArch.Domain.TodoLists;

public static class TodoListErrors
{
    public static Error NotFound(int todoListId) =>
        Error.NotFound(
            "TodoList.NotFound",
            $"The to-do list with the Id = '{todoListId}' was not found"
        );
}

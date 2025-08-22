namespace CleanArch.Application.TodoItems.DeleteTodoItem;

internal sealed class DeleteTodoListCommandValidator : AbstractValidator<DeleteTodoItemCommand>
{
    public DeleteTodoListCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

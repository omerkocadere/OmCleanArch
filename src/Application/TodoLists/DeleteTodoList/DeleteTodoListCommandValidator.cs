namespace CleanArch.Application.TodoLists.DeleteTodoList;

internal sealed class DeleteTodoListCommandValidator : AbstractValidator<DeleteTodoListCommand>
{
    public DeleteTodoListCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

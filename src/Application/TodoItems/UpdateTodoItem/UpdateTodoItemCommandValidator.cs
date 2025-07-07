namespace CleanArch.Application.TodoItems.UpdateTodoItem;

public class UpdateTodoItemCommandValidator : AbstractValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

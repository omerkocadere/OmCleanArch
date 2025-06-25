namespace CleanArch.Application.TodoItems.CreateTodoItem;

public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title).MaximumLength(200).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Note).NotEmpty();
        RuleFor(c => c.DueDate).GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue);
    }
}

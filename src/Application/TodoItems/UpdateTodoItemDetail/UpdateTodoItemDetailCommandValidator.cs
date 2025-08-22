using CleanArch.Application.TodoItems.UpdateTodoItemDetail;

namespace CleanArch.Application.TodoItems.Commands.UpdateTodoItemDetail;

public class UpdateTodoItemDetailCommandValidator : AbstractValidator<UpdateTodoItemDetailCommand>
{
    public UpdateTodoItemDetailCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Title).MaximumLength(200).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Note).NotEmpty();
        RuleFor(c => c.DueDate).GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue);
    }
}

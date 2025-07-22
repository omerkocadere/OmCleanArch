using CleanArch.Application.Common.Interfaces;

namespace CleanArch.Application.TodoLists.CreateTodoList;

public class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoListCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200)
            .Matches("^[a-zA-Z]+$")
            .WithMessage("Only letters are allowed.")
            .MustAsync(BeUniqueTitle)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");
    }

    public async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)
    {
        return !await _context.TodoLists.AnyAsync(l => l.Title == title, cancellationToken);
    }
}

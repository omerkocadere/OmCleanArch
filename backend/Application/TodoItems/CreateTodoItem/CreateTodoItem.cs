using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoItems.CreateTodoItem;

public record CreateTodoItemCommand() : IRequest<Result<int>>
{
    public int ListId { get; init; }
    public required string Title { get; init; }
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; }
    public int UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

public class CreateTodoItemCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTodoItemCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateTodoItemCommand request,
        CancellationToken cancellationToken
    )
    {
        User? user = await context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<int>(UserErrors.NotFound(request.UserId));
        }

        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Note = request.Note,
            Done = false,
            Priority = request.Priority,
            UserId = user.Id,
            Description = request.Description,
            DueDate = request.DueDate,
            Labels = request.Labels,
        };

        entity.Equals(user);

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));

        context.TodoItems.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

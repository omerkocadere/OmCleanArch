using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoItems;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoItems.CreateTodoItem;

public record CreateTodoItemCommand() : IRequest<Result<TodoItemDto>>
{
    public int ListId { get; init; }
    public required string Title { get; init; }
    public string? Note { get; init; }
    public PriorityLevel Priority { get; init; }
    public Guid UserId { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

public class CreateTodoItemCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : IRequestHandler<CreateTodoItemCommand, Result<TodoItemDto>>
{
    public async Task<Result<TodoItemDto>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        User? user = await context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId && u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<TodoItemDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = request.Adapt<TodoItem>();
        entity.UserId = user.Id;

        entity.AddDomainEvent(new TodoItemCreatedEvent(Guid.NewGuid(), entity));

        context.TodoItems.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var dto = entity.Adapt<TodoItemDto>();
        return dto;
    }
}

using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.DTOs;
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

public class CreateTodoItemCommandHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<CreateTodoItemCommand, Result<TodoItemDto>>
{
    public async Task<Result<TodoItemDto>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        User? user = await context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<TodoItemDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = mapper.Map<TodoItem>(request);
        entity.UserId = user.Id;

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));

        context.TodoItems.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<TodoItemDto>(entity);
        return dto;
    }
}

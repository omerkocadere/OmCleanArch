using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoItems;

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

public class CreateTodoItemCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IIdentityService identityService
) : IRequestHandler<CreateTodoItemCommand, Result<TodoItemDto>>
{
    public async Task<Result<TodoItemDto>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists and matches current user context
        var user = await identityService.GetUserByIdAsync(request.UserId);
        if (user is null || userContext.UserId != request.UserId)
        {
            return Result.Failure<TodoItemDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = request.Adapt<TodoItem>();
        entity.UserId = request.UserId;

        entity.AddDomainEvent(new TodoItemCreatedEvent(Guid.NewGuid(), entity));

        context.TodoItems.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var dto = entity.Adapt<TodoItemDto>();
        return dto;
    }
}

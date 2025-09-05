using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoLists.CreateTodoList;

public record CreateTodoListCommand(string Title, Guid UserId) : IRequest<Result<TodoListDto>>;

public class CreateTodoListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTodoListCommand, Result<TodoListDto>>
{
    public async Task<Result<TodoListDto>> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists without loading the full entity
        bool userExists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Result.Failure<TodoListDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = new TodoList
        {
            Title = request.Title,
            UserId = request.UserId,
            User = null!, // Will be populated by EF Core through the foreign key
        };

        context.TodoLists.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        var dto = entity.Adapt<TodoListDto>();
        return dto;
    }
}

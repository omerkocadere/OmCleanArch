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
        User? user = await context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<TodoListDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = new TodoList { Title = request.Title, User = user };

        context.TodoLists.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        var dto = entity.Adapt<TodoListDto>();
        return dto;
    }
}

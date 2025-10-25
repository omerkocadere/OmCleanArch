using CleanArch.Application.Common.Errors;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoLists.CreateTodoList;

public class CreateTodoListCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    : IRequestHandler<CreateTodoListCommand, Result<TodoListDto>>
{
    public async Task<Result<TodoListDto>> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists using IIdentityService
        var user = await identityService.GetUserByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<TodoListDto>(UserErrors.NotFound(request.UserId));
        }

        var entity = new TodoList { Title = request.Title, UserId = request.UserId };

        context.TodoLists.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        var dto = entity.Adapt<TodoListDto>();
        return dto;
    }
}

using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoLists;
using CleanArch.Domain.Users;

namespace CleanArch.Application.TodoLists.CreateTodoList;

public record CreateTodoListCommand(string Title, int UserId) : IRequest<Result<int>>;

public class CreateTodoListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTodoListCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateTodoListCommand request,
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

        var entity = new TodoList { Title = request.Title, UserId = request.UserId };

        context.TodoLists.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

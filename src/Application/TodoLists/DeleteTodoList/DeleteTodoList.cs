using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoLists;

namespace CleanArch.Application.TodoLists.DeleteTodoList;

public record DeleteTodoListCommand(int Id) : IRequest<Result>;

public class DeleteTodoListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteTodoListCommand, Result>
{
    public async Task<Result> Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TodoLists.Where(l => l.Id == request.Id).SingleOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return Result.Failure(TodoListErrors.NotFound(request.Id));
        }

        context.TodoLists.Remove(entity);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

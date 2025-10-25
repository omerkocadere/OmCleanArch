using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.UpdateTodoItem;

public class UpdateTodoItemCommandHandler(IApplicationDbContext context, ICurrentUser userContext)
    : IRequestHandler<UpdateTodoItemCommand, Result>
{
    public async Task<Result> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems.SingleOrDefaultAsync(
            t => t.Id == request.Id && t.UserId == userContext.UserId,
            cancellationToken
        );

        if (todoItem is null)
        {
            return Result.Failure(TodoItemErrors.NotFound(request.Id));
        }

        todoItem.Done = request.Done;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

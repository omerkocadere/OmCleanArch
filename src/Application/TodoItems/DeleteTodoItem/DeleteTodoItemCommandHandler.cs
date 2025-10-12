using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.DeleteTodoItem;

public class DeleteTodoItemCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : IRequestHandler<DeleteTodoItemCommand, Result>
{
    public async Task<Result> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems.SingleOrDefaultAsync(
            t => t.Id == request.Id && t.UserId == userContext.UserId,
            cancellationToken
        );

        if (todoItem is null)
        {
            return Result.Failure(TodoItemErrors.NotFound(request.Id));
        }

        context.TodoItems.Remove(todoItem);

        todoItem.AddDomainEvent(new TodoItemDeletedEvent(Guid.NewGuid(), todoItem));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

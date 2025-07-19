using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.DeleteTodoItem;

public record DeleteTodoItemCommand(int Id) : IRequest<Result>;

public class DeleteTodoItemCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteTodoItemCommand, Result>
{
    public async Task<Result> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems.SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

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

using CleanArch.Application.Common.Interfaces;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.UpdateTodoItemDetail;

public class UpdateTodoItemDetailCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTodoItemDetailCommand, Result>
{
    public async Task<Result> Handle(UpdateTodoItemDetailCommand request, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems.SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (todoItem is null)
        {
            return Result.Failure(TodoItemErrors.NotFound(request.Id));
        }

        todoItem.ListId = request.ListId;
        todoItem.Title = request.Title;
        todoItem.Note = request.Note;
        todoItem.Done = false;
        todoItem.UserId = request.UserId;
        todoItem.Description = request.Description;
        todoItem.DueDate = request.DueDate;
        todoItem.Labels = request.Labels;
        todoItem.Priority = request.Priority;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

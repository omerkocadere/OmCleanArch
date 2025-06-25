using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.UpdateTodoItemDetail;

public record UpdateTodoItemDetailCommand() : IRequest<Result>
{
    public int Id { get; init; }
    public int ListId { get; init; }
    public required string Title { get; init; }
    public required int UserId { get; init; }
    public string? Note { get; init; }
    public string? Description { get; set; }
    public PriorityLevel Priority { get; init; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
}

public class UpdateTodoItemDetailCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTodoItemDetailCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTodoItemDetailCommand request,
        CancellationToken cancellationToken
    )
    {
        TodoItem? todoItem = await context.TodoItems.SingleOrDefaultAsync(
            t => t.Id == request.Id,
            cancellationToken
        );

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

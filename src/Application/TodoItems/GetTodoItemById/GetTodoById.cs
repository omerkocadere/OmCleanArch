using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.GetTodoItemById;

public sealed record GetTodoItemByIdQuery(int TodoItemId) : IRequest<Result<TodoItemDto>>;

public class GetTodoItemByIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IRequestHandler<GetTodoItemByIdQuery, Result<TodoItemDto>>
{
    public async Task<Result<TodoItemDto>> Handle(GetTodoItemByIdQuery query, CancellationToken cancellationToken)
    {
        var todo = await context
            .TodoItems.Where(todoItem => todoItem.Id == query.TodoItemId && todoItem.UserId == userContext.UserId)
            .ProjectToType<TodoItemDto>()
            .SingleOrDefaultAsync(cancellationToken);

        if (todo is null)
        {
            return Result.Failure<TodoItemDto>(TodoItemErrors.NotFound(query.TodoItemId));
        }

        return todo;
    }
}

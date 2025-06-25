using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.TodoItems;

namespace CleanArch.Application.TodoItems.GetTodoItemById;

public sealed record GetTodoItemByIdQuery(int TodoItemId) : IRequest<Result<TodoItemDto>>;

public class GetTodoItemByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetTodoItemByIdQuery, Result<TodoItemDto>>
{
    public async Task<Result<TodoItemDto>> Handle(
        GetTodoItemByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        TodoItemDto? todo = await context
            .TodoItems.Where(todoItem => todoItem.Id == query.TodoItemId)
            .ProjectTo<TodoItemDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (todo is null)
        {
            return Result.Failure<TodoItemDto>(TodoItemErrors.NotFound(query.TodoItemId));
        }

        return todo;
    }
}

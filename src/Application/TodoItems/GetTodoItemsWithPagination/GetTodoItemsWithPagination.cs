using CleanArch.Application.Common.Extensions;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.DTOs;

namespace CleanArch.Application.TodoItems.GetTodoItemsWithPagination;

public record GetTodoItemsWithPaginationQuery : QueryParamsBase, IRequest<Result<PaginatedList<TodoItemDto>>>
{
    public int ListId { get; init; }
}

public class GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTodoItemsWithPaginationQuery, Result<PaginatedList<TodoItemDto>>>
{
    public async Task<Result<PaginatedList<TodoItemDto>>> Handle(
        GetTodoItemsWithPaginationQuery request,
        CancellationToken cancellationToken
    )
    {
        var result = await context
            .TodoItems.Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ProjectToType<TodoItemDto>()
            .PageByAsync(request);

        return result;
    }
}

using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.DTOs;

namespace CleanArch.Application.TodoItems.GetTodoItemsWithPagination;

public record GetTodoItemsWithPaginationQuery : IRequest<Result<PaginatedList<TodoItemDto>>>
{
    public int ListId { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}

public class GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTodoItemsWithPaginationQuery, Result<PaginatedList<TodoItemDto>>>
{
    public async Task<Result<PaginatedList<TodoItemDto>>> Handle(
        GetTodoItemsWithPaginationQuery request,
        CancellationToken cancellationToken
    )
    {
        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? 10;

        var result = await context
            .TodoItems.Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ProjectToType<TodoItemDto>()
            .PaginatedListAsync(pageNumber, pageSize);

        return result;
    }
}

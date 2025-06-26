using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Mappings;
using CleanArch.Application.Common.Models;

namespace CleanArch.Application.TodoItems.GetTodoItemsWithPagination;

public record GetTodoItemsWithPaginationQuery : IRequest<Result<PaginatedList<TodoItemDto>>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
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
            .ProjectTo<TodoItemDto>(mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);

        return result;
    }
}

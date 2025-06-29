using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoItems.CreateTodoItem;
using CleanArch.Application.TodoItems.DeleteTodoItem;
using CleanArch.Application.TodoItems.DTOs;
using CleanArch.Application.TodoItems.GetTodoItemById;
using CleanArch.Application.TodoItems.GetTodoItemsWithPagination;
using CleanArch.Application.TodoItems.UpdateTodoItem;
using CleanArch.Application.TodoItems.UpdateTodoItemDetail;
using CleanArch.Web.Extensions;

namespace CleanArch.Web.Endpoints;

public class TodoItems : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetTodoItemsWithPagination)
            .MapGet(GetTodoItemById, "{id}")
            .MapPost(CreateTodoItem)
            .MapPut(UpdateTodoItem, "{id}")
            .MapPut(UpdateTodoItemDetail, "UpdateDetail/{id}")
            .MapDelete(DeleteTodoItem, "{id}");
    }

    public async Task<IResult> GetTodoItemsWithPagination(
        ISender sender,
        [AsParameters] GetTodoItemsWithPaginationQuery query
    )
    {
        var result = await sender.Send(query);

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetTodoItemById(ISender sender, int id)
    {
        var result = await sender.Send(new GetTodoItemByIdQuery(id));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> CreateTodoItem(ISender sender, CreateTodoItemCommand command)
    {
        Result<TodoItemDto> result = await sender.Send(command);

        return result.Match(
            dto => Results.CreatedAtRoute(nameof(GetTodoItemById), new { id = dto.Id }, dto),
            CustomResults.Problem
        );
    }

    public async Task<IResult> UpdateTodoItem(ISender sender, int id, UpdateTodoItemCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        Result result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public async Task<IResult> UpdateTodoItemDetail(
        ISender sender,
        int id,
        UpdateTodoItemDetailCommand command
    )
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        Result result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public async Task<IResult> DeleteTodoItem(ISender sender, int id)
    {
        Result result = await sender.Send(new DeleteTodoItemCommand(id));

        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

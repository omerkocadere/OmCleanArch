using CleanArch.Application.Common.Models;
using CleanArch.Application.TodoLists.CreateTodoList;
using CleanArch.Application.TodoLists.DeleteTodoList;
using CleanArch.Application.TodoLists.GetTodos;
using CleanArch.Application.TodoLists.UpdateTodoList;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class TodoLists : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .RequireAuthorization()
            .MapGet(GetTodoLists, "{userId:guid}")
            .MapPost(CreateTodoList)
            .MapPut(UpdateTodoList, "{id}")
            .MapDelete(DeleteTodoList, "{id}");
    }

    public async Task<IResult> GetTodoLists(ISender sender, Guid userId)
    {
        var result = await sender.Send(new GetTodosQuery(userId));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> CreateTodoList(ISender sender, CreateTodoListCommand command)
    {
        var result = await sender.Send(command);
        return result.Match(dto => Results.Created(string.Empty, dto), CustomResults.Problem);
    }

    public async Task<IResult> UpdateTodoList(ISender sender, int id, UpdateTodoListCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        Result result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    public async Task<IResult> DeleteTodoList(ISender sender, int id)
    {
        var result = await sender.Send(new DeleteTodoListCommand(id));

        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

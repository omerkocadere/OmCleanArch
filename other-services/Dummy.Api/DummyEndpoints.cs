using Dummy.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dummy.Api;

public static class DummyEndpoints
{
    public static void MapDummyEndpoints(this WebApplication app)
    {
        app.MapGet("/hellofromdummy", HelloFromDummyHandler);
        app.MapGet("/todoitems", GetAllTodoItemsAsync);
    }

    private static string HelloFromDummyHandler()
    {
        return "Hello, world from Dummy API!";
    }

    private static async Task<IResult> GetAllTodoItemsAsync([FromServices] ApplicationDbContext db)
    {
        var items = await db.TodoItems.ToListAsync();
        return Results.Ok(items);
    }
}

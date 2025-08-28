using CleanArch.Application.WeatherForecasts.GetWeatherForecasts;
using CleanArch.Domain.Common;
using CleanArch.Infrastructure.Data;
using CleanArch.Web.Api.Extensions;
using CleanArch.Web.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Api.Endpoints;

public class TestApis : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetWeatherForecasts, "{id:int}");
        groupBuilder.MapGet(GetDummyMessage, "dummy-message");
        groupBuilder.MapGet(GetToDoItemsFromDummyApi, "todo-items");
    }

    public async Task<IResult> GetWeatherForecasts(ISender sender, int id)
    {
        var result = await sender.Send(new GetWeatherForecastsQuery());
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetDummyMessage(ApplicationDbContext context, DummyApiClient dummyApiClient)
    {
        await context.Users.ToListAsync();
        var result = await dummyApiClient.GetHelloMessageAsync();
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetToDoItemsFromDummyApi(ApplicationDbContext context, DummyApiClient dummyApiClient)
    {
        var result = await dummyApiClient.GetToDoItemsAsync();
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

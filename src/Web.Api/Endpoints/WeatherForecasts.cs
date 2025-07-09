using CleanArch.Application.WeatherForecasts.GetWeatherForecasts;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).MapGet(GetWeatherForecasts, "{id:int}");
    }

    public async Task<IResult> GetWeatherForecasts(ISender sender, int id)
    {
        var result = await sender.Send(new GetWeatherForecastsQuery());

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

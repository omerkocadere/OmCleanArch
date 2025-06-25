using CleanArch.Application.WeatherForecasts.GetWeatherForecasts;
using CleanArch.Web.Extensions;

namespace CleanArch.Web.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).MapGet(GetWeatherForecasts);
    }

    public async Task<IResult> GetWeatherForecasts(ISender sender)
    {
        var result = await sender.Send(new GetWeatherForecastsQuery());

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

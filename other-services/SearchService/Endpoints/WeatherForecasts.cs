using SearchService.Models;

namespace SearchService.Endpoints;

public static class WeatherForecasts
{
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Hot"];

    public static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("", GetWeatherForecast).WithTags("WeatherForecast");
    }

    private static WeatherForecast[] GetWeatherForecast()
    {
        var forecast = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();

        return forecast;
    }
}

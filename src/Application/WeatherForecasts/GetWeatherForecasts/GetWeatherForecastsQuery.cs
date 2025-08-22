using CleanArch.Domain.Common;

namespace CleanArch.Application.WeatherForecasts.GetWeatherForecasts;

public record GetWeatherForecastsQuery : IRequest<Result<IEnumerable<WeatherForecast>>>;

public class GetWeatherForecastsQueryHandler
    : IRequestHandler<GetWeatherForecastsQuery, Result<IEnumerable<WeatherForecast>>>
{
    private static readonly string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching",
    ];

    public Task<Result<IEnumerable<WeatherForecast>>> Handle(
        GetWeatherForecastsQuery request,
        CancellationToken cancellationToken
    )
    {
        var rng = new Random();

        var forecasts = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)],
            });

        return Task.FromResult(Result.Success(forecasts));
    }
}

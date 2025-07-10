using CleanArch.Application.Common.Models;
using CleanArch.Application.WeatherForecasts.GetWeatherForecasts;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).MapGet(GetWeatherForecasts, "{id:int}").MapPost(CreateEmail, "create-email");
    }

    public async Task<IResult> GetWeatherForecasts(ISender sender, int id)
    {
        var result = await sender.Send(new GetWeatherForecastsQuery());

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public IResult CreateEmail(ISender sender, string email)
    {
        var result = Email.Create(email);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

public class Email
{
    public const int MaxLength = 5;
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        return Result
            .Create(email)
            .Ensure(e => !string.IsNullOrWhiteSpace(e), EmailErrors.EmailEmpty)
            .Ensure(e => e.Length <= MaxLength, EmailErrors.EmailTooLong)
            .Ensure(e => e.Split('@').Length == 2, EmailErrors.EmailInvalid)
            .Map(e => new Email(e));
    }

    public static class EmailErrors
    {
        public static readonly Error EmailEmpty = Error.Problem("Users.EmailEmpty", "Email is required.");

        public static readonly Error EmailTooLong = Error.Problem(
            "Users.EmailTooLong",
            "The email address is too long"
        );

        public static readonly Error EmailInvalid = Error.Problem(
            "Users.EmailInvalid",
            "The provided email is not valid"
        );
    }
}

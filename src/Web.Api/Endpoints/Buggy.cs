using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Buggy : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetNotFound, "not-found")
            .MapGet(GetBadRequest, "bad-request")
            .MapGet(GetUnauthorized, "unauthorized")
            .MapGet(GetValidationError, "validation-error")
            .MapGet(GetServerError, "server-error");
    }

    public IResult GetNotFound()
    {
        return Results.NotFound();
    }

    public IResult GetBadRequest()
    {
        return Results.BadRequest("This is not a good request");
    }

    public IResult GetUnauthorized()
    {
        return Results.Unauthorized();
    }

    public IResult GetValidationError()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Problem1", new[] { "This is the first error" } },
            { "Problem2", new[] { "This is the second error" } },
        };
        return Results.ValidationProblem(errors);
    }

    public IResult GetServerError()
    {
        throw new Exception("This is a server error");
    }
}

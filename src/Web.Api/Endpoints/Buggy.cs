using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Buggy : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapGet(GetNotFoundCustom, "not-found-custom")
            .MapGet(GetBadRequestCustom, "bad-request-custom")
            .MapGet(GetUnauthorizedCustom, "unauthorized-custom")
            .MapGet(GetValidationErrorCustom, "validation-error-custom")
            .MapGet(GetFailureCustom, "failure-custom")
            .MapGet(GetExceptionCustom, "server-error-custom")
            .MapGet(GetNotFound, "not-found")
            .MapGet(GetBadRequest, "bad-request")
            .MapGet(GetUnauthorized, "unauthorized")
            .MapGet(GetValidationError, "validation-error")
            .MapGet(GetServerError, "server-error");
    }

    public IResult GetNotFoundCustom()
    {
        var result = Result.Failure(Error.NotFound("NotFound", "Resource not found"));
        return CustomResults.Problem(result);
    }

    public IResult GetBadRequestCustom()
    {
        var result = Result.Failure(Error.Problem("BadRequest", "This is not a good request"));
        return CustomResults.Problem(result);
    }

    public IResult GetUnauthorizedCustom()
    {
        var result = Result.Failure(Error.Unauthorized("Unauthorized", "You are not authorized"));
        return CustomResults.Problem(result);
    }

    public IResult GetValidationErrorCustom()
    {
        var validationError = new ValidationError(
            [
                Error.Validation("Problem1", "This is the first error"),
                Error.Validation("Problem2", "This is the second error"),
            ]
        );
        var result = Result.Failure(validationError);
        return CustomResults.Problem(result);
    }

    public IResult GetFailureCustom()
    {
        var result = Result.Failure(Error.Failure("ServerError", "This is a server error"));
        return CustomResults.Problem(result);
    }

    public IResult GetExceptionCustom()
    {
        throw new Exception("This is a real server error (exception)");
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

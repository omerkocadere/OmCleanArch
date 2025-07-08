using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class BuggyCustomResult : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetNotFoundCustom, "not-found-custom")
            .MapGet(GetBadRequestCustom, "bad-request-custom")
            .MapGet(GetUnauthorizedCustom, "unauthorized-custom")
            .MapGet(GetValidationErrorCustom, "validation-error-custom")
            .MapGet(GetServerErrorCustom, "server-error-custom");
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

    public IResult GetServerErrorCustom()
    {
        var result = Result.Failure(Error.Failure("ServerError", "This is a server error"));
        return CustomResults.Problem(result);
    }
}

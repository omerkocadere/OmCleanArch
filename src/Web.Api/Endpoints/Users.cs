using CleanArch.Application.Users.DTOs;
using CleanArch.Application.Users.GetAll;
using CleanArch.Application.Users.GetByEmail;
using CleanArch.Application.Users.GetById;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetById, "{id:guid}").RequireAuthorization();
        groupBuilder.MapGet(GetByEmail, "by-email/{email}").RequireAuthorization();
        groupBuilder.MapGet(GetAll, "").RequireAuthorization();
    }

    public static async Task<IResult> GetById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetUserByIdQuery(id));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public static async Task<IResult> GetByEmail(ISender sender, string email)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public static async Task<IResult> GetAll(ISender sender)
    {
        var result = await sender.Send(new GetAllUsersQuery());

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

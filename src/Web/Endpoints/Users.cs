using CleanArch.Application.Users.GetByEmail;
using CleanArch.Application.Users.GetById;
using CleanArch.Web.Extensions;

namespace CleanArch.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this).MapGet(GetById, "{id:int}").MapGet(GetByEmail, "by-email/{email}");
    }

    public async Task<IResult> GetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetUserByIdQuery(id));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetByEmail(ISender sender, string email)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email));

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

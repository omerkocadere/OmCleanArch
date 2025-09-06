using CleanArch.Application.Account.Commands.Login;
using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Account : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Register, "register");
        groupBuilder.MapPost(Login, "login");
    }

    public static async Task<IResult> Login(ISender sender, LoginCommand command)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(user => Results.Ok(user), CustomResults.Problem);
    }

    public static async Task<IResult> Register(ISender sender, RegisterCommand command)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(
            dto => Results.CreatedAtRoute("GetUserById", new { id = dto.Id }, dto),
            CustomResults.Problem
        );
    }
}

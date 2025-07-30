using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.Create;
using CleanArch.Application.Users.DTOs;
using CleanArch.Application.Users.GetAll;
using CleanArch.Application.Users.GetByEmail;
using CleanArch.Application.Users.GetById;
using CleanArch.Application.Users.Login;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Register, "register");
        groupBuilder.MapPost(Login, "login");

        groupBuilder.MapGet(GetById, "{id:guid}").RequireAuthorization();
        groupBuilder.MapGet(GetByEmail, "by-email/{email}").RequireAuthorization();
        groupBuilder.MapGet(GetAll, "").RequireAuthorization();
    }

    public async Task<IResult> Login(ISender sender, LoginCommand command)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(user => Results.Ok(user), CustomResults.Problem);
    }

    public async Task<IResult> Register(ISender sender, CreateUserCommand command)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(
            dto => Results.CreatedAtRoute(nameof(GetById), new { id = dto.Id }, dto),
            CustomResults.Problem
        );
    }

    public async Task<IResult> GetById(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetUserByIdQuery(id));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetByEmail(ISender sender, string email)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email));

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    public async Task<IResult> GetAll(ISender sender)
    {
        var result = await sender.Send(new GetAllUsersQuery());

        return result.Match(Results.Ok, CustomResults.Problem);
    }
}

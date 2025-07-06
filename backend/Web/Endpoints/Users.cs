using CleanArch.Application.Common.Models;
using CleanArch.Application.Users.CreateUser;
using CleanArch.Application.Users.DTOs;
using CleanArch.Application.Users.GetByEmail;
using CleanArch.Application.Users.GetById;
using CleanArch.Infrastructure.OpenTelemetry;
using CleanArch.Web.Extensions;

namespace CleanArch.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetById, "{id:int}")
            .MapGet(GetByEmail, "by-email/{email}")
            .MapPost(CreateUser);
    }

    public async Task<IResult> CreateUser(ISender sender, CreateUserCommand command)
    {
        Result<UserDto> result = await sender.Send(command);

        var entry = result.Value;

        DiagnosticsConfig.CreatedUserCounter.Add(
            1,
            new KeyValuePair<string, object?>("user.name", entry.FirstName),
            new KeyValuePair<string, object?>("user.id", entry.Id),
            new KeyValuePair<string, object?>("user.lastName", entry.LastName)
        );

        return result.Match(
            dto => Results.CreatedAtRoute(nameof(GetById), new { id = dto.Id }, dto),
            CustomResults.Problem
        );
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

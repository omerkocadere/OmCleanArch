using CleanArch.Application.Admin.Commands;
using CleanArch.Application.Admin.Queries;
using CleanArch.Domain.Constants;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Admin : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapGet("users-with-roles", GetUsersWithRoles)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        groupBuilder
            .MapPost("edit-roles/{userId}", EditUserRoles)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        groupBuilder
            .MapGet("photos-to-moderate", GetPhotosForModeration)
            .RequireAuthorization(AuthorizationPolicies.ModeratePhotoRole);
    }

    private static async Task<IResult> GetUsersWithRoles(ISender sender)
    {
        var query = new GetUsersWithRolesQuery();
        var result = await sender.Send(query);

        return result.Match(onSuccess: Results.Ok, onFailure: CustomResults.Problem);
    }

    private static async Task<IResult> EditUserRoles(Guid userId, string roles, ISender sender)
    {
        var command = new EditUserRolesCommand(userId, roles);
        var result = await sender.Send(command);

        return result.Match(onSuccess: Results.Ok, onFailure: CustomResults.Problem);
    }

    private static IResult GetPhotosForModeration()
    {
        return Results.Ok("Admins or moderators can see this");
    }
}

using CleanArch.Application.Admin.ApprovePhoto;
using CleanArch.Application.Admin.EditUserRole;
using CleanArch.Application.Admin.GetPhotosForModeration;
using CleanArch.Application.Admin.GetUsersWithRoles;
using CleanArch.Application.Admin.RejectPhoto;
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
        groupBuilder
            .MapPost("approve-photo/{photoId}", ApprovePhoto)
            .RequireAuthorization(AuthorizationPolicies.ModeratePhotoRole);
        groupBuilder
            .MapPost("reject-photo/{photoId}", RejectPhoto)
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

    private static async Task<IResult> GetPhotosForModeration(ISender sender)
    {
        var query = new GetPhotosForModerationQuery();
        var result = await sender.Send(query);

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> ApprovePhoto(Guid photoId, ISender sender)
    {
        var command = new ApprovePhotoCommand(photoId);
        var result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    private static async Task<IResult> RejectPhoto(Guid photoId, ISender sender)
    {
        var command = new RejectPhotoCommand(photoId);
        var result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

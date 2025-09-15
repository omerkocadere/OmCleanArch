using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Photos.Commands.DeletePhoto;
using CleanArch.Application.Photos.Commands.SetMainPhoto;
using CleanArch.Application.Photos.Commands.UploadPhoto;
using CleanArch.Application.Photos.DTOs;
using CleanArch.Application.Photos.Queries.GetMemberPhotos;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Photos : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet("/{id:guid}", GetMemberPhotos).Produces<List<PhotoDto>>();
        groupBuilder.MapPost("upload", AddPhoto).DisableAntiforgery();
        groupBuilder.MapPut("set-main-photo/{photoId:guid}", SetMainPhoto);
        groupBuilder.MapDelete("delete-photo/{photoId:guid}", DeletePhoto);
    }

    private static async Task<IResult> GetMemberPhotos(Guid id, IMediator mediator, IUserContext userContext)
    {
        var isCurrentUser = userContext.UserId == id;
        var result = await mediator.Send(new GetMemberPhotosQuery(id, isCurrentUser));
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> AddPhoto(IFormFile file, ISender sender)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded");

        using var fileStream = file.OpenReadStream();

        var fileDto = new FileDto(fileStream, file.FileName, file.ContentType, file.Length);
        var command = new UploadPhotoCommand(fileDto);
        Result<PhotoDto> result = await sender.Send(command);

        return result.Match(Results.Ok, CustomResults.Problem);
    }

    private static async Task<IResult> DeletePhoto(Guid photoId, ISender sender)
    {
        var command = new DeletePhotoCommand(photoId);
        Result result = await sender.Send(command);

        return result.Match(() => Results.Ok(new { Message = "Photo deleted successfully" }), CustomResults.Problem);
    }

    private static async Task<IResult> SetMainPhoto(Guid photoId, ISender sender)
    {
        var command = new SetMainPhotoCommand(photoId);
        Result result = await sender.Send(command);

        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}

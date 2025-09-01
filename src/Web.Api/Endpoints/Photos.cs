using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Models;
using CleanArch.Web.Api.Common;

namespace CleanArch.Web.Api.Endpoints;

public class Photos : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapPost("upload", AddPhoto)
            .DisableAntiforgery() // Required for file uploads
            .WithSummary("Upload a photo to Cloudinary")
            .WithDescription("Uploads a photo file and returns Cloudinary URL");

        groupBuilder
            .MapDelete("{publicId}", DeletePhoto)
            .WithSummary("Delete a photo from Cloudinary")
            .WithDescription("Deletes a photo using its public ID");
    }

    private static async Task<IResult> AddPhoto(IFormFile file, IPhotoService photoService)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded");

        // Convert IFormFile to PhotoUploadRequest (Clean Architecture mapping)
        using var fileStream = file.OpenReadStream();
        var photoRequest = new PhotoUploadRequest
        {
            FileStream = fileStream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
        };

        try
        {
            var result = await photoService.UploadPhotoAsync(photoRequest);

            if (result.Error != null)
                return Results.BadRequest($"Upload failed: {result.Error.Message}");

            return Results.Ok(
                new
                {
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId,
                    Width = result.Width,
                    Height = result.Height,
                }
            );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Upload error: {ex.Message}");
        }
    }

    private static async Task<IResult> DeletePhoto(string publicId, IPhotoService photoService)
    {
        try
        {
            var result = await photoService.DeletePhotoAsync(publicId);

            if (result.Error != null)
                return Results.BadRequest($"Delete failed: {result.Error.Message}");

            return Results.Ok(new { Message = "Photo deleted successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Delete error: {ex.Message}");
        }
    }
}

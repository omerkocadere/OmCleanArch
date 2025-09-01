using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Messaging;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Common;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record UploadPhotoCommand(PhotoUploadRequest PhotoRequest) : ICommand<PhotoUploadResult>;

public record PhotoUploadResult(
    string Url,
    string PublicId,
    int Width,
    int Height
);

public class UploadPhotoCommandHandler(IPhotoService photoService) : ICommandHandler<UploadPhotoCommand, PhotoUploadResult>
{
    public async Task<Result<PhotoUploadResult>> Handle(UploadPhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            ImageUploadResult result = await photoService.UploadPhotoAsync(request.PhotoRequest);

            if (result.Error != null)
            {
                return Result.Failure<PhotoUploadResult>(
                    new Error("Photo.UploadFailed", $"Upload failed: {result.Error.Message}")
                );
            }

            var photoResult = new PhotoUploadResult(
                result.SecureUrl.ToString(),
                result.PublicId,
                result.Width,
                result.Height
            );

            return Result.Success(photoResult);
        }
        catch (Exception ex)
        {
            return Result.Failure<PhotoUploadResult>(
                new Error("Photo.UploadError", $"Upload error: {ex.Message}")
            );
        }
    }
}

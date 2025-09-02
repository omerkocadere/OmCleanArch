using CleanArch.Application.Photos.Commands.UploadPhoto;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Common.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> UploadPhotoAsync(FileDto request);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}

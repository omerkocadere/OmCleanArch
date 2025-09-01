using CleanArch.Application.Common.Models;
using CloudinaryDotNet.Actions;

namespace CleanArch.Application.Common.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> UploadPhotoAsync(PhotoUploadRequest request);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}

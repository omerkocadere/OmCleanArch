using CleanArch.Application.Photos.DTOs;

namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record UploadPhotoCommand(FileDto FileDto) : ICommand<PhotoDto>;

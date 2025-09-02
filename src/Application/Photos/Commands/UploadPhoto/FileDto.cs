namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record FileDto(Stream FileStream, string FileName, string ContentType, long FileSize);

namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record PhotoDto(string Url, string PublicId, Guid MemberId);

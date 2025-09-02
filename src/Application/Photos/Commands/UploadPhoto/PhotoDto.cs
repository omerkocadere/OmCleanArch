namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public record PhotoDto(Guid Id, string Url, string PublicId, Guid MemberId);

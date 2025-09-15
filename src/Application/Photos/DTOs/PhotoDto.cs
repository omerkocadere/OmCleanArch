namespace CleanArch.Application.Photos.DTOs;

public record PhotoDto(Guid Id, string Url, string? PublicId, Guid MemberId, bool IsApproved = true);

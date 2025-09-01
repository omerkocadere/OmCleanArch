namespace CleanArch.Application.Common.Models;

public class PhotoUploadRequest
{
    public required Stream FileStream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long FileSize { get; init; }
}

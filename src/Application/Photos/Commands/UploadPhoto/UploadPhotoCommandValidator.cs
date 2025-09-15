namespace CleanArch.Application.Photos.Commands.UploadPhoto;

public class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public UploadPhotoCommandValidator()
    {
        RuleFor(x => x.FileDto).NotNull().WithMessage("Photo upload request is required.");

        RuleFor(x => x.FileDto.FileStream)
            .NotNull()
            .WithMessage("File stream is required.")
            .When(x => x.FileDto != null);

        RuleFor(x => x.FileDto.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .Must(fileName => !string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            .WithMessage("File must have a valid extension.")
            .When(x => x.FileDto != null);

        RuleFor(x => x.FileDto.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.")
            .Must(contentType => AllowedImageTypes.Contains(contentType.ToLowerInvariant()))
            .WithMessage($"Only image files are allowed. Supported types: {string.Join(", ", AllowedImageTypes)}")
            .When(x => x.FileDto != null);

        RuleFor(x => x.FileDto.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB.")
            .When(x => x.FileDto != null);
    }
}

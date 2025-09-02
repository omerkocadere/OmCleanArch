using CleanArch.Domain.Common;

namespace CleanArch.Domain.Photos;

public static class PhotoErrors
{
    public static readonly Error UploadFailed = Error.Failure("Photos.UploadFailed", "Photo upload failed.");

    public static readonly Error UploadError = Error.Failure(
        "Photos.UploadError",
        "An error occurred during photo upload."
    );

    public static readonly Error DeleteFailed = Error.Failure("Photos.DeleteFailed", "Photo deletion failed.");

    public static readonly Error DeleteError = Error.Failure(
        "Photos.DeleteError",
        "An error occurred during photo deletion."
    );
}

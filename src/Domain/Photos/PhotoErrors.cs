namespace CleanArch.Domain.Photos;

public static class PhotoErrors
{
    public static readonly Error NotFound = Error.NotFound("Photos.NotFound", "Photo not found");

    public static readonly Error CannotSetMain = Error.Failure("Photos.CannotSetMain", "Cannot set this as main image");

    public static readonly Error CannotDelete = Error.Failure("Photos.CannotDelete", "This photo cannot be deleted");

    public static readonly Error PhotoNotApproved = Error.Failure("Photos.PhotoNotApproved", "Only approved photos can be set as main");

    public static readonly Error AlreadyApproved = Error.Failure("Photos.AlreadyApproved", "Photo is already approved");

    public static readonly Error AlreadyRejected = Error.Failure("Photos.AlreadyRejected", "Photo is already rejected");
}

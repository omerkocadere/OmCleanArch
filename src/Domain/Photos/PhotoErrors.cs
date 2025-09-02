namespace CleanArch.Domain.Photos;

public static class PhotoErrors
{


    public static readonly Error CannotSetMain = Error.Failure("Photos.CannotSetMain", "Cannot set this as main image");

    public static readonly Error CannotDelete = Error.Failure("Photos.CannotDelete", "This photo cannot be deleted");
}

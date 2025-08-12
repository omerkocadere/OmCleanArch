using CleanArch.Domain.Common;

namespace CleanArch.Domain.Members;

public static class MemberErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Members.NotFound",
        "The member with the specified identifier was not found."
    );

    public static readonly Error NoPhotos = Error.NotFound(
        "Members.NoPhotos",
        "No photos found for the specified member."
    );
}

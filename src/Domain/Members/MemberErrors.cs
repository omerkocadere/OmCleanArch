namespace CleanArch.Domain.Members;

public static class MemberErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Members.NotFound",
        "The member with the specified identifier was not found."
    );

    // Like-related errors
    public static readonly Error LikeNotFound = Error.NotFound(
        "Members.LikeNotFound",
        "The like relationship was not found."
    );

    public static readonly Error LikeAlreadyExists = Error.Conflict(
        "Members.LikeAlreadyExists",
        "You have already liked this member."
    );

    public static readonly Error CannotLikeSelf = Error.Validation(
        "Members.CannotLikeSelf",
        "You cannot like yourself."
    );
}

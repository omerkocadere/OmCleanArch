namespace CleanArch.Application.Common.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Authentication.InvalidCredentials",
        "The provided email or password is incorrect."
    );

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Authentication.InvalidRefreshToken",
        "Invalid or expired refresh token."
    );

    public static readonly Error ExpiredRefreshToken = Error.Unauthorized(
        "Authentication.ExpiredRefreshToken",
        "The refresh token has expired. Please log in again."
    );

    public static readonly Error RefreshTokenNotFound = Error.Unauthorized(
        "Authentication.RefreshTokenNotFound",
        "No valid refresh token found for the user."
    );

    public static readonly Error SessionExpired = Error.Unauthorized(
        "Authentication.SessionExpired",
        "Your session has expired. Please log in again."
    );

    public static readonly Error Unauthorized = Error.Unauthorized(
        "Authentication.Unauthorized",
        "You are not authorized to access this resource."
    );

    public static readonly Error TokenGenerationFailed = Error.Problem(
        "Authentication.TokenGenerationFailed",
        "Failed to generate authentication tokens."
    );

    public static readonly Error LoginFailed = Error.Problem(
        "Authentication.LoginFailed",
        "Authentication failed. Please try again."
    );
}

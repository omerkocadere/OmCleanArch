using System.Security.Claims;

namespace CleanArch.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid parsedUserId) ? parsedUserId : null;
    }

    public static string? GetUserName(this ClaimsPrincipal? principal)
    {
        // For IdentityServer, try "username" claim first (as configured in CustomProfileService)
        string? userName = principal?.FindFirstValue("username");

        // If not found, try the standard name claim
        if (string.IsNullOrEmpty(userName))
        {
            userName = principal?.FindFirstValue(ClaimTypes.Name);
        }

        // If still not found, try the JwtClaimTypes.Name (from IdentityModel)
        if (string.IsNullOrEmpty(userName))
        {
            userName = principal?.FindFirstValue("name");
        }

        return userName;
    }
}

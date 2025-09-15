using CleanArch.Application.Account.Commands.Login;
using CleanArch.Application.Account.Commands.RefreshToken;
using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Web.Api.Extensions;

namespace CleanArch.Web.Api.Endpoints;

public class Account : EndpointGroupBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Register, "register");
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(RefreshToken, "refresh-token");
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    public static async Task<IResult> Login(ISender sender, LoginCommand command, HttpContext httpContext)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(
            userDto =>
            {
                // Web layer responsibility: Set cookie (infrastructure concern)
                if (userDto.RefreshToken != null && userDto.RefreshTokenExpiry.HasValue)
                {
                    SetRefreshTokenCookie(httpContext, userDto.RefreshToken, userDto.RefreshTokenExpiry.Value);
                }
                return Results.Ok(userDto);
            },
            error => CustomResults.Problem(error)
        );
    }

    public static async Task<IResult> Register(ISender sender, RegisterCommand command, HttpContext httpContext)
    {
        Result<UserDto> result = await sender.Send(command);

        return result.Match(
            userDto =>
            {
                // Web layer responsibility: Set cookie (infrastructure concern)
                if (userDto.RefreshToken != null && userDto.RefreshTokenExpiry.HasValue)
                {
                    SetRefreshTokenCookie(httpContext, userDto.RefreshToken, userDto.RefreshTokenExpiry.Value);
                }
                return Results.CreatedAtRoute("GetUserById", new { id = userDto.Id }, userDto);
            },
            error => CustomResults.Problem(error)
        );
    }

    public static async Task<IResult> RefreshToken(ISender sender, HttpContext httpContext)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (refreshToken == null)
        {
            return Results.NoContent();
        }

        var command = new RefreshTokenCommand(refreshToken);
        var result = await sender.Send(command);

        return result.Match(
            userDto =>
            {
                // Web layer responsibility: Set cookie (infrastructure concern)
                if (userDto.RefreshToken != null && userDto.RefreshTokenExpiry.HasValue)
                {
                    SetRefreshTokenCookie(httpContext, userDto.RefreshToken, userDto.RefreshTokenExpiry.Value);
                }
                return Results.Ok(userDto);
            },
            error => CustomResults.Problem(error)
        );
    }

    public static async Task<IResult> Logout(HttpContext httpContext, IIdentityService identityService)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Single DB operation: Find and invalidate token atomically
            await identityService.InvalidateRefreshTokenAsync(refreshToken);
        }

        // Clear the refresh token cookie
        httpContext.Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            }
        );

        return Results.Ok(new { message = "Logged out successfully" });
    }

    // Web layer concern: Cookie management only
    private static void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, DateTime expiry)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiry,
        };

        httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }
}

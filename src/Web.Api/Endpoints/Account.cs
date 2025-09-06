using CleanArch.Application.Account.Commands.Login;
using CleanArch.Application.Account.Commands.RefreshToken;
using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using CleanArch.Web.Api.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<IResult> RefreshToken(
        ISender sender,
        HttpContext httpContext,
        UserManager<User> userManager
    )
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

    public static async Task<IResult> Logout(HttpContext httpContext, UserManager<User> userManager)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Find user by refresh token and invalidate it
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = DateTime.UtcNow; // Expire immediately
                user.RefreshTokenCreatedAt = null; // Clear creation time
                await userManager.UpdateAsync(user);
            }
        }

        // Clear the refresh token cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1), // Expire in the past
        };

        httpContext.Response.Cookies.Append(RefreshTokenCookieName, string.Empty, cookieOptions);

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

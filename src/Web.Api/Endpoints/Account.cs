using CleanArch.Application.Account.Commands.Login;
using CleanArch.Application.Account.Commands.Register;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Application.Users.DTOs;
using CleanArch.Domain.Common;
using CleanArch.Domain.Users;
using CleanArch.Web.Api.Extensions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Web.Api.Endpoints;

public class Account : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Register, "register");
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(RefreshToken, "refresh-token");
    }

    public static async Task<IResult> Login(
        ISender sender,
        LoginCommand command,
        HttpContext httpContext,
        UserManager<User> userManager,
        ITokenProvider tokenService
    )
    {
        Result<UserDto> result = await sender.Send(command);

        return await result.Match(
            async user =>
            {
                var appUser = await userManager.FindByEmailAsync(command.Email);
                if (appUser != null)
                {
                    await SetRefreshTokenCookie(appUser, httpContext, userManager, tokenService);
                }
                return Results.Ok(user);
            },
            error => Task.FromResult(CustomResults.Problem(error))
        );
    }

    public static async Task<IResult> Register(
        ISender sender,
        RegisterCommand command,
        HttpContext httpContext,
        UserManager<User> userManager,
        ITokenProvider tokenService
    )
    {
        Result<UserDto> result = await sender.Send(command);

        return await result.Match(
            async dto =>
            {
                var appUser = await userManager.FindByEmailAsync(command.Email);
                if (appUser != null)
                {
                    await SetRefreshTokenCookie(appUser, httpContext, userManager, tokenService);
                }
                return Results.CreatedAtRoute("GetUserById", new { id = dto.Id }, dto);
            },
            error => Task.FromResult(CustomResults.Problem(error))
        );
    }

    public static async Task<IResult> RefreshToken(
        HttpContext httpContext,
        UserManager<User> userManager,
        ITokenProvider tokenService
    )
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];
        if (refreshToken == null)
        {
            return Results.NoContent();
        }

        var user = await userManager.Users.FirstOrDefaultAsync(x =>
            x.RefreshToken == refreshToken && x.RefreshTokenExpiry > DateTime.UtcNow
        );

        if (user == null)
        {
            return Results.Unauthorized();
        }

        await SetRefreshTokenCookie(user, httpContext, userManager, tokenService);

        var userDto = user.Adapt<UserDto>();
        userDto.Token = await tokenService.CreateAsync(user);

        return Results.Ok(userDto);
    }

    private static async Task SetRefreshTokenCookie(
        User user,
        HttpContext httpContext,
        UserManager<User> userManager,
        ITokenProvider tokenService
    )
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        httpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

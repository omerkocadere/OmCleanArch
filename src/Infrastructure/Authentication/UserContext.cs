using System.Security.Claims;
using CleanArch.Application.Common.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace CleanArch.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid? UserId => httpContextAccessor.HttpContext?.User.GetUserId();

    public string? UserName => httpContextAccessor.HttpContext?.User.GetUserName();
}

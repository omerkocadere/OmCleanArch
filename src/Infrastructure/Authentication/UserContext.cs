using CleanArch.Application.Common.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace CleanArch.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId => httpContextAccessor.HttpContext?.User.GetUserId();

    public string? UserName => httpContextAccessor.HttpContext?.User.GetUserName();

    public string? Email => httpContextAccessor.HttpContext?.User.GetEmail();

    public string? FirstName => httpContextAccessor.HttpContext?.User.GetFirstName();

    public string? LastName => httpContextAccessor.HttpContext?.User.GetLastName();
}

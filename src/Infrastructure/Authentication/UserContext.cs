using CleanArch.Application.Common.Interfaces.Authentication;
using Microsoft.AspNetCore.Http;

namespace CleanArch.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor.HttpContext?.User.GetUserId()
        ?? throw new ApplicationException("User context is unavailable");
}

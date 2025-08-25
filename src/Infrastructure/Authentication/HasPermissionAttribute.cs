using Microsoft.AspNetCore.Authorization;

namespace CleanArch.Infrastructure.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(Permission permission)
    : AuthorizeAttribute(policy: permission.ToString()) { }

using CleanArch.Application.Common.Interfaces.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure.Authentication;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}

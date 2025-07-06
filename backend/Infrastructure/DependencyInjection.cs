using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IHostEnvironment env,
        IConfiguration configuration
    )
    {
        return services
            .AddServices()
            .AddDatabase(env) // Now uses DatabaseConfiguration
            .AddBackgroundJobs()
            .ConfigureOpenTelemetry(configuration)
            .AddAuthenticationInternal();
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        return services;
    }
}

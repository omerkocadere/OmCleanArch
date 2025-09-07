using System.Text;
using CleanArch.Application.Auctions.Consumers;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.Interfaces.Authentication;
using CleanArch.Domain.Constants;
using CleanArch.Infrastructure.Authentication;
using CleanArch.Infrastructure.Authorization;
using CleanArch.Infrastructure.BackgroundJobs;
using CleanArch.Infrastructure.Data;
using CleanArch.Infrastructure.Idempotence;
using CleanArch.Infrastructure.Identity;
using CleanArch.Infrastructure.Options;
using CleanArch.Infrastructure.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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
            .AddPhotoServices(configuration)
            .AddCacheServices(configuration)
            .AddDatabase(env, configuration)
            .AddBackgroundJobsConditionally(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal()
            .AddMediatRDecorators()
            .AddMassTransitServices(configuration);
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ITelemetryService, TelemetryService>();

        return services;
    }

    private static IServiceCollection AddPhotoServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPhotoService, PhotoService>();
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
        return services;
    }

    private static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure cache options
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddMemoryCache();

        var cacheOptions = configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();

        if (cacheOptions.Provider.Equals(CacheProviders.Redis, StringComparison.OrdinalIgnoreCase))
        {
            // Validate Redis connection string is provided
            if (string.IsNullOrWhiteSpace(cacheOptions.RedisConnectionString))
            {
                throw new InvalidOperationException(
                    "Redis connection string is required when Redis cache provider is selected."
                );
            }

            // Add Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = "CleanArch";
            });

            // Register Redis cache service
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            // Default to memory cache
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));

        // Get authentication options for immediate use
        var authOptions =
            configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
            ?? throw new InvalidOperationException("Authentication configuration is missing or invalid.");

        // Configure authentication based on provider
        switch (authOptions.Provider)
        {
            case AuthenticationProvider.IdentityServer:
                ConfigureIdentityServerAuthentication(services, authOptions.IdentityServer);
                break;

            case AuthenticationProvider.Jwt:
                ConfigureJwtAuthentication(services, authOptions.Jwt);
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication provider: {authOptions.Provider}");
        }

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(UserRoles.Admin))
            .AddPolicy(AuthorizationPolicies.MemberOnly, policy => policy.RequireRole(UserRoles.Member))
            .AddPolicy(AuthorizationPolicies.ModeratorOnly, policy => policy.RequireRole(UserRoles.Moderator))
            .AddPolicy(
                AuthorizationPolicies.ModeratePhotoRole,
                policy => policy.RequireRole(UserRoles.Admin, UserRoles.Moderator)
            );

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<PermissionProvider>();
        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, JwtOptions jwtOptions)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuer = true, // SECURITY: Validate issuer (default: true)
                    ValidateAudience = true, // SECURITY: Validate audience (default: true)
                    ValidateLifetime = true, // SECURITY: Validate token expiration (default: true)
                    ValidateIssuerSigningKey = true, // SECURITY: Validate signing key (default: false - explicitly enabled)
                    ClockSkew = jwtOptions.ClockSkew,
                };
            });
    }

    private static void ConfigureIdentityServerAuthentication(
        IServiceCollection services,
        IdentityServerOptions identityServerOptions
    )
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = identityServerOptions.Authority;
                options.RequireHttpsMetadata = identityServerOptions.RequireHttpsMetadata;
                options.TokenValidationParameters.ValidateAudience = identityServerOptions.ValidateAudience;
                options.TokenValidationParameters.NameClaimType = identityServerOptions.NameClaimType;
            });
    }

    /// <summary>
    /// Registers MediatR decorators using Scrutor for cross-cutting concerns.
    /// This ensures all domain event handlers are wrapped with idempotency logic.
    /// </summary>
    private static IServiceCollection AddMediatRDecorators(this IServiceCollection services)
    {
        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

        return services;
    }

    /// <summary>
    /// Configures MassTransit with RabbitMQ and EntityFramework outbox pattern.
    /// </summary>
    private static IServiceCollection AddMassTransitServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    // RabbitMQ configuration for Docker environments
                    // The application could work with default settings before, but this configuration
                    // is required for proper Docker container communication
                    var rabbitOptions =
                        configuration.GetSection(RabbitMQOptions.SectionName).Get<RabbitMQOptions>()
                        ?? new RabbitMQOptions();

                    cfg.Host(
                        rabbitOptions.Host,
                        rabbitOptions.VirtualHost,
                        h =>
                        {
                            h.Username(rabbitOptions.Username);
                            h.Password(rabbitOptions.Password);
                        }
                    );

                    cfg.ConfigureEndpoints(context);
                }
            );
        });

        return services;
    }
}

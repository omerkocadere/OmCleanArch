using System.ComponentModel.DataAnnotations;

namespace CleanArch.Infrastructure.Options;

/// <summary>
/// Configuration options for authentication settings.
/// Consolidates both JWT and Identity Server authentication configurations.
/// </summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Determines which authentication provider to use.
    /// </summary>
    [Required]
    public AuthenticationProvider Provider { get; set; } = AuthenticationProvider.Jwt;

    /// <summary>
    /// JWT-based authentication configuration.
    /// Used when Provider is set to Jwt.
    /// </summary>
    [Required]
    public JwtOptions Jwt { get; set; } = new();

    /// <summary>
    /// Identity Server authentication configuration.
    /// Used when Provider is set to IdentityServer.
    /// </summary>
    [Required]
    public IdentityServerOptions IdentityServer { get; set; } = new();
}

public enum AuthenticationProvider
{
    Jwt,
    IdentityServer,
}

public sealed class JwtOptions
{
    [Required]
    [MinLength(8, ErrorMessage = "JWT Secret must be at least 8 characters long")]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 10080, ErrorMessage = "Expiration must be between 1 and 10080 minutes (1 week)")]
    public int ExpirationInMinutes { get; set; } = 60;

    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;
}

public sealed class IdentityServerOptions
{
    [Required]
    [Url(ErrorMessage = "IdentityServiceUrl must be a valid URL")]
    public string Authority { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = false;

    public bool ValidateAudience { get; set; } = false;

    public string NameClaimType { get; set; } = "username";
}

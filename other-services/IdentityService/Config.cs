using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [new IdentityResources.OpenId(), new IdentityResources.Profile()];

    public static IEnumerable<ApiScope> ApiScopes => [new ApiScope("auctionApp", "Auction App Full Access")];

    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                ClientSecrets = [new Secret("NotASecret".Sha256())],
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
                AllowedScopes = { "openid", "profile", "auctionApp" },
                RedirectUris = { GetPostmanRedirectUri() },
            },
            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                RedirectUris = { GetNextAppRedirectUri() },
                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "auctionApp" },
                AccessTokenLifetime = 3600 * 24 * 30,
            },
        ];

    private static string GetPostmanRedirectUri()
    {
        // Ideally, fetch from configuration/environment; fallback to default if not set
        var uri = Environment.GetEnvironmentVariable("POSTMAN_REDIRECT_URI");
        return !string.IsNullOrWhiteSpace(uri) ? uri : "https://www.getpostman.com/oauth2/callback"; // fallback for local/dev
    }

    private static string GetNextAppRedirectUri()
    {
        // Fetch from environment variable, fallback to default for local/dev
        var uri = Environment.GetEnvironmentVariable("NEXTAPP_REDIRECT_URI");
        return !string.IsNullOrWhiteSpace(uri) ? uri : "http://localhost:3000/api/auth/callback/id-server";
    }
}
